using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoLight : MonoBehaviour
{
    public bool AutoStart = true;
    public Material ballMaterial;

    // public GameObject SpotLightPrefab;
    [Header("Animation--------------------------------")]
    public float speedRotX;
    public float speedRotY;
    float oldSpeedRotX, oldSpeedRotY;
    Tweener rotXTweener, rotYTweener;
    public Ease rotX = Ease.Unset;
    public Ease rotY = Ease.Unset;
    Ease oldRotX = Ease.Unset;
    Ease oldRotY = Ease.Unset;

    [Header("Pose--------------------------------")]
    public int sliceX = 4;
    public int sliceY = 4;
    public float radius = 0.5f;
    public float intensity, angle, range;
    public Color[] colorList;
    Pose[] poses;

    [Header("light beam----------------------------------")]
    public GameObject lightBeamPrefab;
    public Material lightBeamMaterial;
    public float lightBeamRange;
   // public float lightBeamRadiusTop;
    public float lightBeamRadiusBottom;
    public float lightBeamAlpha;
    List<LineRenderer> lightBeamList;

    Pose[] _createLightPoints()
    {
        List<Pose> vecs = new List<Pose>();

        ////y loop
        for (int y = 0; y < sliceY; y++)
        {
            float ratioY = y / (float)sliceY * Mathf.PI * 0.5f;

            ////draw circle
            for (float a = 0; a < sliceX; a++)
            {
                float ratio;
                if (y == 0)
                {
                    ratio = a / (float)sliceX * Mathf.PI * 2f;
                    Vector3 posStart = new Vector3(
                        Mathf.Sin(ratio) * Mathf.Cos(ratioY) * radius,
                        Mathf.Sin(ratioY) * radius,
                        Mathf.Cos(ratio) * Mathf.Cos(ratioY) * radius);
                    //GL.Vertex3(posStart.x, posStart.y, posStart.z);
                    vecs.Add(new Pose(posStart, Quaternion.LookRotation(-posStart)));
                }

                //ratio = (a + 1f) / (float)slice * Mathf.PI * 2f;
                //posEnd = new Vector3(
                //    startX + Mathf.Sin(ratio) * Mathf.Cos(ratioY) * radius,
                //    startY + Mathf.Sin(ratioY) * radius,
                //    startZ + Mathf.Cos(ratio) * Mathf.Cos(ratioY) * radius);
                //posEnd = matrix.MultiplyPoint(posEnd);
                //GL.Vertex3(posEnd.x, posEnd.y, posEnd.z);

                ////draw y line
                //GL.Vertex3(posStart.x, posStart.y, posStart.z);

                ratio = a / (float)sliceX * Mathf.PI * 2f;
                float nextRatioY = (y + 1) / (float)sliceY * Mathf.PI * 0.5f;
                Vector3 posEnd = new Vector3(
                    Mathf.Sin(ratio) * Mathf.Cos(nextRatioY) * radius,
                    Mathf.Sin(nextRatioY) * radius,
                    Mathf.Cos(ratio) * Mathf.Cos(nextRatioY) * radius);
                //GL.Vertex3(posEnd.x, posEnd.y, posEnd.z);
                vecs.Add(new Pose(posEnd, Quaternion.LookRotation(-posEnd)));

                //opposit position
                posEnd.y *= -1;
                vecs.Add(new Pose(posEnd, Quaternion.LookRotation(-posEnd)));

                if (y == sliceY - 1)
                    break;
            }
        }
        return vecs.ToArray();
    }

    List<Light> slightList;
    float oldBeamRange, oldBeamTop, oldBeamBottom, oldBeamAlpha;

    void Update()
    {
        if (slightList == null &&
            (Input.GetKeyDown(KeyCode.A) || AutoStart))
        {
            Pose[] lightPose = _createLightPoints();
            slightList = new List<Light>();
            foreach (Pose pose in lightPose)
            {
                GameObject obj = new GameObject("LightUnit");//GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.localScale = Vector3.one * 1.0f;
                obj.transform.parent = transform;
                obj.transform.localPosition = pose.position;
                obj.transform.localRotation = pose.rotation;

                //if (SpotLightPrefab!=null)
                {
                    GameObject slightObj = new GameObject("spotlight");// Instantiate(SpotLightPrefab) ;
                    slightObj.transform.parent = obj.transform;
                    slightObj.transform.localPosition = Vector3.zero;
                    slightObj.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    Light slight = slightObj.AddComponent<Light>();
                    slight.type = LightType.Spot;
                    slight.enabled = true;
                    slightList.Add(slight);
                }

                //Create light model
                GameObject lightModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lightModel.transform.parent = obj.transform;
                lightModel.transform.localPosition = Vector3.zero;
                lightModel.transform.localRotation = Quaternion.Euler(90, 0, 0);
                lightModel.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f) * (radius / 0.5f);
                Collider col = lightModel.GetComponent<Collider>();
                DestroyImmediate(col);
                MeshRenderer renderer = lightModel.GetComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Unlit/Color"));
            }

            //create big ball
            GameObject bigBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);//new GameObject("BigBall");
            bigBall.name = "BigBall";
            bigBall.transform.parent = transform;//.parent.parent.parent;
            bigBall.transform.localPosition = Vector3.zero;
            bigBall.transform.localRotation = Quaternion.identity;
            bigBall.transform.localScale = Vector3.one * radius * 2;
            Collider colider = bigBall.GetComponent<Collider>();
            Destroy(colider);
            if (ballMaterial != null)
                bigBall.GetComponent<Renderer>().material = ballMaterial;
        }

        if (slightList != null)
        {
            int count = 0;
            foreach (Light light in slightList)
            {
                light.intensity = intensity;
                light.spotAngle = angle;
                light.range = range;
                if (colorList != null && colorList.Length > 0)
                    light.color = colorList[(int)Mathf.Repeat(count, colorList.Length)];

                //if (oldBeamAlpha != lightBeamAlpha)
                {
                    //Lightbeam beam = light.GetComponentInChildren<Lightbeam>();
                    Transform beam;
                    MyHelpNode.FindTransform(light.transform, "Lightbeam2D", out beam, true);
                    if (beam != null)
                    {
                        Renderer beamRenderer = beam.GetComponent<Renderer>();
                        beamRenderer.material.color = new Color(light.color.r, light.color.g, light.color.b, lightBeamAlpha);
                    }
                }

                count++;
            }
            //oldBeamAlpha = lightBeamAlpha;
        }

        if (lightBeamPrefab != null && lightBeamList == null && slightList != null)
        {
            lightBeamList = new List<LineRenderer>();
            foreach (Light light in slightList)
            {
                GameObject lightBeamObj = Instantiate(lightBeamPrefab);
                lightBeamObj.transform.parent = light.transform;
                lightBeamObj.transform.localPosition = Vector3.zero;
                lightBeamObj.transform.localRotation = Quaternion.Euler(Vector3.right * -90);// Quaternion.LookRotation(Vector3.forward, Vector3.up);
                LineRenderer lightBeam = lightBeamObj.GetComponent<LineRenderer>();
                lightBeam.GetComponent<Renderer>().material = new Material(lightBeamMaterial);
                lightBeamList.Add(lightBeam);
            }
        }

        if (lightBeamList != null)
        {
            if ((oldBeamRange != lightBeamRange) ||
                //(oldBeamTop != lightBeamRadiusTop) ||
                (oldBeamBottom != lightBeamRadiusBottom))
            {
                oldBeamRange = lightBeamRange;
                //oldBeamTop = lightBeamRadiusTop;
                oldBeamBottom = lightBeamRadiusBottom;
                foreach (LineRenderer beam in lightBeamList)
                {                  
                    beam.SetPosition(0, Vector3.up* 0.02f);
                    beam.SetPosition(1, Vector3.up * lightBeamRange);
                    beam.widthMultiplier = lightBeamRadiusBottom;
                    //beam.endWidth = lightBeamRadiusTop;
                }
            }
        }

        if (oldRotX != rotX || oldSpeedRotX != speedRotX)
        {
            DOTween.Kill(transform.parent);
            oldSpeedRotX = speedRotX;
            oldRotX = rotX;
            transform.parent.DOLocalRotate(Vector3.right * 360, speedRotX, RotateMode.LocalAxisAdd).SetLoops(-1, LoopType.Restart).SetEase(rotX);
        }

        if (oldRotY != rotY || oldSpeedRotY != speedRotY)
        {
            DOTween.Kill(transform.parent.parent);
            oldSpeedRotY = speedRotY;
            oldRotY = rotY;
            transform.parent.parent.DOLocalRotate(Vector3.up * 360, speedRotY, RotateMode.LocalAxisAdd).SetLoops(-1, LoopType.Restart).SetEase(rotY);
        }
    }

    private void OnEnable()
    {
        if (slightList != null)
        {
            foreach (Light l in slightList)
                l.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (slightList != null)
        {
            foreach (Light l in slightList)
                l.enabled = false;
        }
    }
}
