
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using UdonSharp;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class OkSync : UdonSharpBehaviour
{
    public Transform canvasSyncAnchor;
    public Texture2D tex;
    public GameObject syncImageDisplay;
    public Camera syncCam;
    public VRC_Pickup displayPickup;
    public OkSync[] subSyncs;
    public OkSync mainSync;
    public GameObject[] displayChunks;
    public Camera displayCam;
    public TextMeshProUGUI syncButtonLabel;
    public GameObject syncCooldownCover;
    public Transform copyRigScaleTransform;
    public Animator revealAnimator;
    public GameObject model;
    public bool pickupable = true;
    private float displayChunkTimer;
    private bool activateAfterTransfer;
    private int printIndex;
    private float printDelay;
    private bool LocalPlayerOwnsThis { get { return Networking.IsOwner(Networking.LocalPlayer, gameObject); } }
    private byte[][] imageDataBuffers;
    private Color32[][] capturedPixels;
    private readonly int indexByteOffset = 5880;
    private readonly int bufferCount = 40;
    private readonly int chunkWidth = 280;
    private readonly int chunkHeight = 7;
    private readonly int chunkPixelCount = (280 * 280) / 40;
    private int processSequence = -1;
    private bool buffersInitialized;
    private Color32[] chunkDisplayBuffer;
    private int captureSequence = -1;
    private float cooldown = -1.0f;
    private bool[] pendingWork = new bool[40];
    private float workTimer = -99.0f;
    private readonly float workPeriod = 0.4f;
    private int dataIndex;
    public bool DisplayIsActive { get { return model.activeSelf; } }
    [UdonSynced, FieldChangeCallback(nameof(ImageData))] private byte[] _imageData; public byte[] ImageData { get { return _imageData; } set { _imageData = value; ImageDataUpdate(); } }

    private void Start()
    {
        ImageData = _imageData;
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (LocalPlayerOwnsThis && activateAfterTransfer)
        {
            activateAfterTransfer = false;
            Activate();
        }

        printIndex = 0;
        printDelay = 0.0f;
    }

    public void ImageDataUpdate()
    {
        if (ImageData != null && ImageData.Length > 1 && !LocalPlayerOwnsThis)
        {
            if (mainSync != null)
            {
                dataIndex = ImageData[indexByteOffset];
                mainSync.ScheduleWork(dataIndex - 1);
            }
        }
    }

    public void ScheduleWork(int index)
    {
        Unhide();
        pendingWork[index] = true;
        if (workTimer < 0.0f)
        {
            workTimer = workPeriod;
        }
    }

    public void ScheduleAllWork()
    {
        Unhide();
        for (int i = 0; i < pendingWork.Length; i++)
        {
            pendingWork[i] = true;
        }
        if (workTimer < 0.0f)
        {
            workTimer = workPeriod;
        }
    }

    public void DoWork()
    {
        if (chunkDisplayBuffer == null)
        {
            chunkDisplayBuffer = new Color32[chunkPixelCount];
        }

        for (int i = 0; i < chunkPixelCount; i++)
        {
            chunkDisplayBuffer[i].r = ImageData[i * 3];
            chunkDisplayBuffer[i].g = ImageData[i * 3 + 1];
            chunkDisplayBuffer[i].b = ImageData[i * 3 + 2];
        }

        tex.SetPixels32(0, 0, chunkWidth, chunkHeight, chunkDisplayBuffer);
        tex.Apply(false);

        displayChunkTimer = workPeriod / 2.0f;
        mainSync.displayChunks[39 - (dataIndex - 1)].SetActive(true);
        mainSync.displayCam.enabled = true;
    }

    public void ResetDisplayPosition()
    {
        Unhide();

        syncImageDisplay.transform.position = canvasSyncAnchor.position;
        syncImageDisplay.transform.rotation = canvasSyncAnchor.rotation;
        syncImageDisplay.transform.Translate(new Vector3(0.12f, 0.12f, 0.012f), Space.Self);
    }

    public void Unhide()
    {
        if (!DisplayIsActive)
        {
            model.SetActive(true);
            syncImageDisplay.transform.GetChild(0).gameObject.SetActive(true);
            displayPickup.pickupable = pickupable;
        }
    }

    public void ImageSizeUpdate(float newScale)
    {
        syncImageDisplay.transform.localScale = new Vector3(newScale, newScale, 1.0f);
        copyRigScaleTransform.transform.localScale = new Vector3(1.0f / newScale, 1.0f / newScale, 1.0f);
    }

    public void ResetDisplaySize()
    {
        cooldown = 27.0f;
        syncButtonLabel.text = "Sync In\nProgress...";
        syncCooldownCover.SetActive(true);
    }

    public void Activate()
    {
        if ((ImageData != null && ImageData.Length > 1) || printIndex > 0 || cooldown > 0.0f)
        {
            return;
        }

        if (LocalPlayerOwnsThis)
        {
            transform.position = canvasSyncAnchor.position;
            transform.rotation = canvasSyncAnchor.rotation;
            syncCam.enabled = true;
            captureSequence = 0;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ResetDisplaySize));
        }
        else
        {
            activateAfterTransfer = true;
            for (int i = 0; i < subSyncs.Length; i++)
            {
                Networking.SetOwner(Networking.LocalPlayer, subSyncs[i].gameObject);
            }
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void OnPostRender()
    {
        tex.ReadPixels(new Rect(0, captureSequence * chunkHeight, tex.width, tex.height), 0, 0);
        tex.Apply();
        if (capturedPixels == null)
        {
            capturedPixels = new Color32[40][];
        }
        capturedPixels[captureSequence] = tex.GetPixels32();

        captureSequence += 1;
        if (captureSequence == 40)
        {
            syncCam.enabled = false;
            processSequence = buffersInitialized ? 100 : 0;
        }
    }

    public void SetImageData(byte[] bytes)
    {
        ImageData = bytes;
        dataIndex = ImageData[indexByteOffset];
        RequestSerialization();
    }

    public void Update()
    {
        // Disable sync button during cooldown
        if (cooldown > 0.0f)
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0.0f)
            {
                cooldown = -1.0f;
                syncButtonLabel.text = "Sync\nCopy";
                syncCooldownCover.SetActive(false);
            }
        }

        // Display camera
        if (displayChunkTimer > 0.0f)
        {
            displayChunkTimer -= Time.deltaTime;
            if (displayChunkTimer <= 0.0f)
            {
                mainSync.displayChunks[39 - (dataIndex - 1)].SetActive(false);
                mainSync.displayCam.enabled = false;
            }
        }

        // Process image capture
        if (processSequence >= 0)
        {
            // Initialize buffers
            if (!buffersInitialized)
            {
                if (imageDataBuffers == null)
                {
                    imageDataBuffers = new byte[bufferCount + 1][];
                }

                imageDataBuffers[processSequence + 1] = new byte[chunkPixelCount * 3 + 1];
                processSequence += 1;
                if (processSequence >= bufferCount)
                {
                    buffersInitialized = true;
                    processSequence = 100;
                }
            }

            // Write buffers
            if (processSequence >= 100)
            {
                var d = processSequence - 100;
                var currentBuffer = imageDataBuffers[d + 1];
                for (int i = 0; i < chunkPixelCount; i++)
                {
                    var pixel = capturedPixels[d][i];
                    currentBuffer[i * 3] = pixel.r;
                    currentBuffer[i * 3 + 1] = pixel.g;
                    currentBuffer[i * 3 + 2] = pixel.b;
                }

                processSequence += 1;
                if (processSequence - 100 >= bufferCount)
                {
                    // Write data index to last byte
                    for (byte i = 1; i <= bufferCount; i++)
                    {
                        imageDataBuffers[i][indexByteOffset] = i;
                    }

                    // Start print sequence
                    processSequence = -1;
                    printIndex = 1;
                    printDelay = 1.0f;
                    revealAnimator.SetTrigger("Show");
                }
            }
        }

        // Print sequence
        if (LocalPlayerOwnsThis && printIndex >= 1)
        {
            if (!DisplayIsActive && printDelay >= 0.999f)
            {
                if (mainSync == null)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ResetDisplayPosition));
                }
            }

            printDelay -= Time.deltaTime;
            if (printDelay <= 0.0f)
            {
                subSyncs[printIndex - 1].SetImageData(imageDataBuffers[printIndex]);

                printIndex += 1;
                if (printIndex > bufferCount)
                {
                    printIndex = 0;
                    ScheduleAllWork();
                }
            }
        }

        // Display chunks
        if (workTimer >= 0.0f)
        {
            workTimer -= Time.deltaTime;
            if (workTimer <= 0.0f)
            {
                for (int i = 0; i < pendingWork.Length; i++)
                {
                    if (pendingWork[i])
                    {
                        pendingWork[i] = false;
                        subSyncs[i].DoWork();
                        workTimer = workPeriod;
                        break;
                    }

                    if (i == pendingWork.Length - 1)
                    {
                        workTimer = -99.0f;
                    }
                }
            }
        }
    }
}
