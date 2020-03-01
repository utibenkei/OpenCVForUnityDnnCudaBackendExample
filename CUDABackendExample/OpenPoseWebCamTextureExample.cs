#if !(PLATFORM_LUMIN && !UNITY_EDITOR)

#if !UNITY_WSA_10_0

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;

namespace OpenCVForUnityExample
{
    /// <summary>
    /// OpenPoseWebCamTextureExample
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class OpenPoseWebCamTextureExample : MonoBehaviour
    {
        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The bgr mat.
        /// </summary>
        Mat bgrMat;

        /// <summary>
        /// The BLOB.
        /// </summary>
        Mat input;

        /// <summary>
        /// The net.
        /// </summary>
        Net net;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;


        public enum DATASET_TYPE
        {
            COCO,
            MPI,
            HAND
        }
        public DATASET_TYPE dataset = DATASET_TYPE.MPI;

        const float inWidth = 368;
        const float inHeight = 368;

        const float inScale = 1.0f / 255f;


        Dictionary<string, int> BODY_PARTS;
        string[,] POSE_PAIRS;

        /// <summary>
        /// CAFFEMODEL_FILENAME
        /// </summary>
        string CAFFEMODEL_FILENAME;

        /// <summary>
        /// The caffemodel filepath.
        /// </summary>
        string caffemodel_filepath;

        /// <summary>
        /// PROTOTXT_FILENAME
        /// </summary>
        string PROTOTXT_FILENAME;

        /// <summary>
        /// The prototxt filepath.
        /// </summary>
        string prototxt_filepath;

#if UNITY_WEBGL && !UNITY_EDITOR
        IEnumerator getFilePath_Coroutine;
#endif

        // Use this for initialization
        void Start ()
        {
            fpsMonitor = GetComponent<FpsMonitor> ();

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();

            if (dataset == DATASET_TYPE.COCO)
            {
                //COCO
                BODY_PARTS = new Dictionary<string, int>() {
                        { "Nose", 0 }, { "Neck", 1 }, { "RShoulder", 2 }, { "RElbow", 3 }, {
                            "RWrist",
                            4
                        },
                        { "LShoulder",5 }, { "LElbow", 6 }, { "LWrist", 7 }, { "RHip", 8 }, {
                            "RKnee",
                            9
                        },
                        { "RAnkle", 10 }, { "LHip", 11 }, { "LKnee", 12 }, { "LAnkle", 13 }, {
                            "REye",
                            14
                        },
                        { "LEye", 15 }, { "REar", 16 }, { "LEar", 17 }, {
                            "Background",
                            18
                        }
                    };

                POSE_PAIRS
                = new string[,] {
                        { "Neck", "RShoulder" }, { "Neck", "LShoulder" }, {
                            "RShoulder",
                            "RElbow"
                        },
                        { "RElbow", "RWrist" }, { "LShoulder", "LElbow" }, {
                            "LElbow",
                            "LWrist"
                        },
                        { "Neck", "RHip" }, { "RHip", "RKnee" }, { "RKnee", "RAnkle" }, {
                            "Neck",
                            "LHip"
                        },
                        { "LHip", "LKnee" }, { "LKnee", "LAnkle" }, { "Neck", "Nose" }, {
                            "Nose",
                            "REye"
                        },
                        { "REye", "REar" }, { "Nose", "LEye" }, { "LEye", "LEar" }
                };

                CAFFEMODEL_FILENAME = "dnn/pose_iter_440000.caffemodel";
                PROTOTXT_FILENAME = "dnn/openpose_pose_coco.prototxt";
            }
            else if (dataset == DATASET_TYPE.MPI)
            {

                //MPI
                BODY_PARTS
                = new Dictionary<string, int>() { { "Head", 0 }, { "Neck", 1 }, {
                        "RShoulder",
                        2
                    }, {
                        "RElbow",
                        3
                    }, {
                        "RWrist",
                        4
                    },
                    { "LShoulder", 5 }, { "LElbow", 6 }, { "LWrist", 7 }, { "RHip", 8 }, {
                        "RKnee",
                        9
                    },
                    { "RAnkle", 10 }, { "LHip", 11 }, { "LKnee", 12 }, { "LAnkle", 13 }, {
                        "Chest",
                        14
                    },
                    { "Background", 15 }
                };

                POSE_PAIRS = new string[,] {
                    { "Head", "Neck" }, {
                        "Neck",
                        "RShoulder"
                    }, {
                        "RShoulder",
                        "RElbow"
                    },
                    { "RElbow", "RWrist" },
                    { "Neck", "LShoulder" }, {
                        "LShoulder",
                        "LElbow"
                    },
                    { "LElbow", "LWrist" },
                    { "Neck", "Chest" },
                    { "Chest", "RHip" }, {
                        "RHip",
                        "RKnee"
                    },
                    { "RKnee", "RAnkle" },
                    { "Chest", "LHip" },
                    { "LHip", "LKnee" }, {
                        "LKnee",
                        "LAnkle"
                    }
                };

                CAFFEMODEL_FILENAME = "dnn/pose_iter_160000.caffemodel";
                PROTOTXT_FILENAME = "dnn/openpose_pose_mpi_faster_4_stages.prototxt";

            }
            else if (dataset == DATASET_TYPE.HAND)
            {
                //HAND
                BODY_PARTS = new Dictionary<string, int>() {{ "Wrist", 0 },
                    { "ThumbMetacarpal", 1 },{ "ThumbProximal", 2 },{ "ThumbMiddle", 3 },{ "ThumbDistal", 4 },
                    { "IndexFingerMetacarpal", 5 }, {"IndexFingerProximal", 6 },{ "IndexFingerMiddle", 7 },{ "IndexFingerDistal", 8 },
                    { "MiddleFingerMetacarpal", 9 },{ "MiddleFingerProximal", 10 },{ "MiddleFingerMiddle", 11 },{ "MiddleFingerDistal", 12 },
                    { "RingFingerMetacarpal", 13 },{ "RingFingerProximal", 14 },{ "RingFingerMiddle", 15 },{ "RingFingerDistal", 16 },
                    { "LittleFingerMetacarpal", 17 }, {"LittleFingerProximal", 18 }, {"LittleFingerMiddle", 19 },{ "LittleFingerDistal", 20 }
                };

                POSE_PAIRS = new string[,] { {"Wrist", "ThumbMetacarpal"}, {"ThumbMetacarpal", "ThumbProximal"},
                   {"ThumbProximal", "ThumbMiddle"}, {"ThumbMiddle", "ThumbDistal"},
                   {"Wrist", "IndexFingerMetacarpal"}, {"IndexFingerMetacarpal", "IndexFingerProximal"},
                   {"IndexFingerProximal", "IndexFingerMiddle"}, {"IndexFingerMiddle", "IndexFingerDistal"},
                   {"Wrist", "MiddleFingerMetacarpal"}, {"MiddleFingerMetacarpal", "MiddleFingerProximal"},
                   {"MiddleFingerProximal", "MiddleFingerMiddle"}, {"MiddleFingerMiddle", "MiddleFingerDistal"},
                   {"Wrist", "RingFingerMetacarpal"}, {"RingFingerMetacarpal", "RingFingerProximal"},
                   {"RingFingerProximal", "RingFingerMiddle"}, {"RingFingerMiddle", "RingFingerDistal"},
                   {"Wrist", "LittleFingerMetacarpal"}, {"LittleFingerMetacarpal", "LittleFingerProximal"},
                   {"LittleFingerProximal", "LittleFingerMiddle"}, {"LittleFingerMiddle", "LittleFingerDistal"} };


                CAFFEMODEL_FILENAME = "dnn/pose_iter_102000.caffemodel";
                PROTOTXT_FILENAME = "dnn/pose_deploy.prototxt";

            }

#if UNITY_WEBGL && !UNITY_EDITOR
            getFilePath_Coroutine = GetFilePath ();
            StartCoroutine (getFilePath_Coroutine);
#else
            caffemodel_filepath = Utils.getFilePath(CAFFEMODEL_FILENAME);
            prototxt_filepath = Utils.getFilePath(PROTOTXT_FILENAME);
            Run();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator GetFilePath ()
        {

            var getFilePathAsync_0_Coroutine = Utils.getFilePathAsync (CAFFEMODEL_FILENAME, (result) => {
                caffemodel_filepath = result;
            });
            yield return getFilePathAsync_0_Coroutine;

            var getFilePathAsync_1_Coroutine = Utils.getFilePathAsync (PROTOTXT_FILENAME, (result) => {
                prototxt_filepath = result;
            });
            yield return getFilePathAsync_1_Coroutine;

            getFilePath_Coroutine = null;

            Run ();
        }
#endif

        // Use this for initialization
        void Run ()
        {
            //if true, The error log of the Native side OpenCV will be displayed on the Unity Editor Console.
            Utils.setDebugMode (true);


            if (string.IsNullOrEmpty(caffemodel_filepath) || string.IsNullOrEmpty(prototxt_filepath))
            {
                Debug.LogError("model file is not loaded. The model and prototxt file can be downloaded here: \"http://posefs1.perception.cs.cmu.edu/OpenPose/models/pose/mpi/pose_iter_160000.caffemodel\",\"https://github.com/opencv/opencv_extra/blob/master/testdata/dnn/openpose_pose_mpi_faster_4_stages.prototxt\". Please copy to “Assets/StreamingAssets/dnn/” folder. ");
            }
            else
            {
                net = Dnn.readNetFromCaffe(prototxt_filepath, caffemodel_filepath);


                //net.setPreferableBackend(Dnn.DNN_BACKEND_OPENCV);
                //net.setPreferableTarget(Dnn.DNN_TARGET_CPU);
                net.setPreferableBackend(Dnn.DNN_BACKEND_CUDA);
                net.setPreferableTarget(Dnn.DNN_TARGET_CUDA);

            }

#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
            webCamTextureToMatHelper.Initialize ();
        }

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();

            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);
            Utils.fastMatToTexture2D(webCamTextureMat, texture);

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null) {
                fpsMonitor.Add ("width", webCamTextureMat.width ().ToString ());
                fpsMonitor.Add ("height", webCamTextureMat.height ().ToString ());
                fpsMonitor.Add ("orientation", Screen.orientation.ToString ());
            }

                                    
            float width = webCamTextureMat.width ();
            float height = webCamTextureMat.height ();
                                    
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }

            bgrMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC3);
        }

        /// <summary>
        /// Raises the webcam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");

            if (bgrMat != null)
                bgrMat.Dispose ();

            if (texture != null) {
                Texture2D.Destroy (texture);
                texture = null;
            }
        }

        /// <summary>
        /// Raises the webcam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred (WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update ()
        {
            if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();


                if (net == null)
                {

                    Imgproc.putText(rgbaMat, "model file is not loaded.", new Point(5, rgbaMat.rows() - 30), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    Imgproc.putText(rgbaMat, "Please read console message.", new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

                }
                else
                {

                    Imgproc.cvtColor(rgbaMat, bgrMat, Imgproc.COLOR_RGBA2BGR);

                    float frameWidth = bgrMat.cols();
                    float frameHeight = bgrMat.rows();

                    input = Dnn.blobFromImage(bgrMat, 1.0 / 255, new Size(inWidth, inHeight), new Scalar(0, 0, 0), false, false);

                    net.setInput(input);

                    //                TickMeter tm = new TickMeter ();
                    //                tm.start ();

                    Mat output = net.forward();

                    //                tm.stop ();
                    //                Debug.Log ("Inference time, ms: " + tm.getTimeMilli ());


                    output = output.reshape(1, 16);


                    float[] data = new float[46 * 46];
                    List<Point> points = new List<Point>();
                    for (int i = 0; i < BODY_PARTS.Count; i++)
                    {

                        output.get(i, 0, data);

                        Mat heatMap = new Mat(1, data.Length, CvType.CV_32FC1);
                        heatMap.put(0, 0, data);


                        //Originally, we try to find all the local maximums. To simplify a sample
                        //we just find a global one. However only a single pose at the same time
                        //could be detected this way.
                        Core.MinMaxLocResult result = Core.minMaxLoc(heatMap);

                        heatMap.Dispose();


                        double x = (frameWidth * (result.maxLoc.x % 46)) / 46;
                        double y = (frameHeight * (result.maxLoc.x / 46)) / 46;

                        if (result.maxVal > 0.1)
                        {
                            points.Add(new Point(x, y));
                        }
                        else
                        {
                            points.Add(null);
                        }

                    }

                    for (int i = 0; i < POSE_PAIRS.GetLength(0); i++)
                    {
                        string partFrom = POSE_PAIRS[i, 0];
                        string partTo = POSE_PAIRS[i, 1];

                        int idFrom = BODY_PARTS[partFrom];
                        int idTo = BODY_PARTS[partTo];

                        if (points[idFrom] != null && points[idTo] != null)
                        {
                            Imgproc.line(rgbaMat, points[idFrom], points[idTo], new Scalar(0, 255, 0, 255), 3);
                            Imgproc.ellipse(rgbaMat, points[idFrom], new Size(3, 3), 0, 0, 360, new Scalar(0, 0, 255, 255), Core.FILLED);
                            Imgproc.ellipse(rgbaMat, points[idTo], new Size(3, 3), 0, 0, 360, new Scalar(0, 0, 255, 255), Core.FILLED);
                        }
                    }

                    output.Dispose();


                    MatOfDouble timings = new MatOfDouble();
                    long t = net.getPerfProfile(timings);
                    //Debug.Log("t: " + t);
                    //Debug.Log("timings.dump(): " + timings.dump());

                    double freq = Core.getTickFrequency() / 1000;
                    //Debug.Log("freq: " + freq);

                    Imgproc.putText(rgbaMat, (t / freq) + "ms", new Point(10, rgbaMat.height() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.6, new Scalar(0, 0, 255, 255), 2);
                }


                Utils.fastMatToTexture2D (rgbaMat, texture);
            }
        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable ()
        {
            webCamTextureToMatHelper.Dispose ();

            if (input != null)
                input.Dispose ();
            if (net != null)
                net.Dispose ();

            if (bgrMat != null)
                bgrMat.Dispose();

            Utils.setDebugMode (false);

            #if UNITY_WEBGL && !UNITY_EDITOR
            if (getFilePath_Coroutine != null) {
                StopCoroutine (getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose ();
            }
            #endif
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {
            SceneManager.LoadScene ("OpenCVForUnityExample");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick ()
        {
            webCamTextureToMatHelper.Play ();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick ()
        {
            webCamTextureToMatHelper.Pause ();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick ()
        {
            webCamTextureToMatHelper.Stop ();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick ()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing ();
        }

        private List<string> readClassNames (string filename)
        {
            List<string> classNames = new List<string> ();

            System.IO.StreamReader cReader = null;
            try {
                cReader = new System.IO.StreamReader (filename, System.Text.Encoding.Default);

                while (cReader.Peek () >= 0) {
                    string name = cReader.ReadLine ();
                    classNames.Add (name);
                }
            } catch (System.Exception ex) {
                Debug.LogError (ex.Message);
                return null;
            } finally {
                if (cReader != null)
                    cReader.Close ();
            }

            return classNames;
        }
    }
}
#endif

#endif