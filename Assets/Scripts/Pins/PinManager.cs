using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PinManager : MonoBehaviour
{
    public static PinManager Instance;

    public GameObject pinPrefab;
    public Transform pinParent;

    private readonly List<Pin> _allPins = new();

    void Awake()
    {
        Instance = this;
        SaveSystem.Init();
    }

    //async void Start()
    //{
    //    await LoadPinsAsync();
    //}

    public async Task LoadPinsAsync()
    {
        var loadedData = await SaveSystem.LoadAsync();

        foreach (var data in loadedData)
        {
            GameObject go = Instantiate(pinPrefab, pinParent);
            Pin pin = go.GetComponent<Pin>();
            pin.SetData(data);
            _allPins.Add(pin);
        }
    }

    public void CreatePin(Vector2 position)
    {
        GameObject go = Instantiate(pinPrefab, pinParent);
        Pin pin = go.GetComponent<Pin>();
        pin.Init(position);
        _allPins.Add(pin);

        SaveAllAsync();
    }

    public async void SaveAllAsync()
    {
        List<PinData> data = new();
        foreach (var pin in _allPins)
            data.Add(pin.Data);

        await SaveSystem.SaveAsync(data);
    }

    public void RemovePin(Pin pin)
    {
        _allPins.Remove(pin);
        Destroy(pin.gameObject);
        SaveAllAsync();
    }
}