using DoplTechnologies.Protos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                _lastCatheterData.Add(data.Clone());
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
                if (!_catheters.TryGetValue(catheterData.SensorId, out tip))
                {
                    Console.WriteLine($"Creating catheter. Sensor id: {catheterData.SensorId}");
                    tip = Instantiate<CatheterTip>(CatheterTipPrefab, new Vector3(0, 0, 0), UnityEngine.Quaternion.identity, CatheterParent);
                    _catheters[catheterData.SensorId] = tip;
                }

                var position = catheterData.Coordinates.Position;
                var rotation = catheterData.Coordinates.Rotation;
                tip.transform.localPosition = new Vector3(
                    position.Y,
                    position.X,
                    position.Z
                );

                tip.transform.localRotation = new UnityEngine.Quaternion(
                    rotation.Y,
                    rotation.X,
                    rotation.Z,
                    rotation.W
                );
            }

            _lastCatheterData = null;
        }
    }
}
