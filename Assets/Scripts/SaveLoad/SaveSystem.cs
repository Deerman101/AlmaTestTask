using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class SaveSystem
{
    private static string _filePath;

    public static void Init()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "pins.json");
    }

    public static async Task SaveAsync(List<PinData> pins)
    {
        string json = JsonUtility.ToJson(new Wrapper { pins = pins }, true);

        await Task.Run(() => File.WriteAllText(_filePath, json, Encoding.UTF8));
    }

    public static async Task<List<PinData>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<PinData>();

        return await Task.Run(() =>
        {
            string json = File.ReadAllText(_filePath, Encoding.UTF8);
            return JsonUtility.FromJson<Wrapper>(json).pins;
        });
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<PinData> pins;
    }
}