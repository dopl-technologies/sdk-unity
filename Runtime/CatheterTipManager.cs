using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DoplConnect;

public class CatheterTipManager : MonoBehaviour
{
    [Tooltip("This prefab is instantiated each time a new catheter is detected")]
    public CatheterTip CatheterTipPrefab;

    [Tooltip("Catheter's will be parented to this game object")]
    public Transform CatheterParent;

    [SerializeField] private DoplConnect _dopl;

    private List<CatheterData> _lastCatheterData;
    private Dictionary<uint, CatheterTip> _catheters = new Dictionary<uint, CatheterTip>();
    private object _catheterDataLock = new object();

    // Start is called before the first frame update
    void Start()
    {
        _dopl.OnCatheterDataEvent += OnCatheterData;
    }

    private void OnCatheterData(CatheterData[] catheters)
    {
        lock(_catheterDataLock)
        {
            _lastCatheterData = new List<CatheterData>();
            foreach(CatheterData data in catheters)
            {
                _lastCatheterData.Add(new CatheterData(data));
            }
        }
    }

    private void Update()
    {
        lock (_catheterDataLock)
        {
            if (_lastCatheterData == null)
                return;

            foreach (CatheterData catheterData in _lastCatheterData)
            {
                CatheterTip tip;
                if (!_catheters.TryGetValue(catheterData.sensorid, out tip))
                {
                    Console.WriteLine($"Creating catheter. Sensor id: {catheterData.sensorid}");
                    tip = Instantiate<CatheterTip>(CatheterTipPrefab, CatheterParent, false);
                    _catheters[catheterData.sensorid] = tip;
                }

                var position = catheterData.coordinates.position;
                var rotation = catheterData.coordinates.rotation;
                tip.transform.localPosition = new Vector3(
                    position.x,
                    -position.y,
                    position.z
                );

                tip.Rotation.transform.localRotation = new UnityEngine.Quaternion(
                    -rotation.x,
                    rotation.y,
                    rotation.z,
                    rotation.w
                );
            }

            _lastCatheterData = null;
        }
    }
}
