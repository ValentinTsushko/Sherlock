using UnityEngine;
using Windows.Kinect;
using System.Linq;

public class LeanControl : MonoBehaviour
{
    private KinectSensor _sensor;
    private BodyFrameReader _bodyFrameReader;
    private Body[] _bodies = null;

    public bool IsAvailable;

    public static LeanControl instance = null;

    public Body[] GetBodies() { return _bodies; }

    public static float LeanX { get; set; }
    public static float LeanY { get; set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        _sensor = KinectSensor.GetDefault();
        if (_sensor != null)
        {
            IsAvailable = _sensor.IsAvailable;
            _bodyFrameReader = _sensor.BodyFrameSource.OpenReader();
            if (!_sensor.IsOpen)
            {
                _sensor.Open();
            }
            _bodies = new Body[_sensor.BodyFrameSource.BodyCount];
        }
    }
    
    void Update()
    {
        IsAvailable = _sensor.IsAvailable;
        if (_bodyFrameReader != null)
        {
            var frame = _bodyFrameReader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.GetAndRefreshBodyData(_bodies);
                foreach (var body in _bodies.Where(b => b.IsTracked))
                {
                    IsAvailable = true;
                    LeanX = body.Lean.X;
                    LeanY = body.Lean.Y;
                }
                frame.Dispose();
                frame = null;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (_bodyFrameReader != null)
        {
            _bodyFrameReader.IsPaused = true;
            _bodyFrameReader.Dispose();
            _bodyFrameReader = null;
        }
        if (_sensor != null)
        {
            if (_sensor.IsOpen)
            {
                _sensor.Close();
            }
            _sensor = null;
        }
    }
}