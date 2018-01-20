using UnityEngine;
using Windows.Kinect;
using System.Linq;

public class KinectManager : MonoBehaviour
{
    private KinectSensor _sensor;
    private BodyFrameReader _bodyFrameReader;
    private Body[] _bodies = null;

    public bool IsAvailable;

    public static KinectManager instance = null;

    public Transform trCharacter;

    public Body[] GetBodies()    { return _bodies; }

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
    
    public enum ControlType
    {
        Lean,
        OpenHand,
        RiseHand,
        RiseLeg
    }

    public ControlType controlType = ControlType.Lean;

    private void LeanControl(Body body)
    {
        trCharacter.localRotation = LeanToRotation(body.Lean.X);
    }

    float curRotationX = 0;
    float rotationSpeed = 0.8f;

    private enum RotationDir
    {
        None,
        Left,
        Right
    }

    private void SetRotationDir(RotationDir dir)
    {
        if (dir == RotationDir.Right)
        {
            curRotationX += Time.deltaTime * rotationSpeed;
            if (curRotationX > 1f)
            {
                curRotationX = 1f;
            }
        }
        else if (dir == RotationDir.Left)
        {
            curRotationX -= Time.deltaTime * rotationSpeed;
            if (curRotationX < -1f)
            {
                curRotationX = -1f;
            }
        }
        else
        {
            if (curRotationX != 0f)
            {
                if (curRotationX > 0f)
                {
                    curRotationX -= rotationSpeed * Time.deltaTime;
                    if (curRotationX < 0f)
                    {
                        curRotationX = 0f;
                    }
                }
                else
                {
                    curRotationX += rotationSpeed * Time.deltaTime;
                    if (curRotationX > 0f)
                    {
                        curRotationX = 0f;
                    }
                }
            }
        }
        Debug.Log("SetRotationDir value: " + curRotationX);
        trCharacter.localRotation = LeanToRotation(curRotationX);
    }

    private void OpenHandControl(Body body)
    {
        if (body.HandRightConfidence == TrackingConfidence.High && body.HandRightState == HandState.Open)
        {
            SetRotationDir(RotationDir.Right);
        }
        else if (body.HandLeftConfidence == TrackingConfidence.High && body.HandLeftState == HandState.Open)
        {
            SetRotationDir(RotationDir.Left);
        } else
        {
            SetRotationDir(RotationDir.None);
        }
    }

    private void RiseHandControl(Body body)
    {
        Debug.Log(string.Format("hand left Y: {0} hand right Y: {1}", body.Joints[JointType.HandLeft].Position.Y, body.Joints[JointType.HandRight].Position.Y));
        float riseHeight = 0.65f;
        if (body.Joints[JointType.HandRight].Position.Y > riseHeight)
        {
            SetRotationDir(RotationDir.Right);
        }
        else if (body.Joints[JointType.HandLeft].Position.Y > riseHeight)
        {
            SetRotationDir(RotationDir.Left);
        }
        else
        {
            SetRotationDir(RotationDir.None);
        }
    }

    private void RiseLegControl(Body body)
    {
        Debug.Log(string.Format("leg left Y: {0} leg right Y: {1}", body.Joints[JointType.FootLeft].Position.Y, body.Joints[JointType.FootRight].Position.Y));
        float riseHeight = -0.7f;
        if (body.Joints[JointType.FootRight].Position.Y > riseHeight)
        {
            SetRotationDir(RotationDir.Right);
        }
        else if (body.Joints[JointType.FootLeft].Position.Y > riseHeight)
        {
            SetRotationDir(RotationDir.Left);
        }
        else
        {
            SetRotationDir(RotationDir.None);
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
                    if (trCharacter == null)
                    {
                        Debug.LogError("trCharacter is NULL");
                    }
                    else
                    {
                        switch(controlType)
                        {
                            case ControlType.Lean:
                                LeanControl(body);
                                break;
                            case ControlType.OpenHand:
                                OpenHandControl(body);
                                break;
                            case ControlType.RiseHand:
                                RiseHandControl(body);
                                break;
                            case ControlType.RiseLeg:
                                RiseLegControl(body);
                                break;
                        }
                    }
                }
                frame.Dispose();
                frame = null;
            }
        }
    }

    private Quaternion LeanToRotation(float rotation)
    {
        rotation *= -1; //mirror rotation
        return Quaternion.Euler(0, rotation * 90f, 0);
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