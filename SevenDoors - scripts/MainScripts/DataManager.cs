using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


[System.Serializable]
public class DataManager
{
    private string file_name = "sevendoors_data.snd";
    private int level_open;//max = 7, def = 1;

    public DataManager()
    {
        if (!File.Exists(Path.Combine(Application.persistentDataPath, file_name)))
            SaveData(1);
    }

    public void DeletData()
    {
        File.Delete(Path.Combine(Application.persistentDataPath, file_name));
    }
    

    public void SaveData(int lvl)
    {
        FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, file_name), FileMode.Create, FileAccess.Write);
        BinaryFormatter bf = new BinaryFormatter();
        level_open = lvl;
        bf.Serialize(stream, this);
        stream.Close();
    }

    public void LoadData()
    {
        FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, file_name), FileMode.Open, FileAccess.Read);

        if (stream is null)
            SaveData(1);
        else
        {
            BinaryFormatter bf = new BinaryFormatter();
            var load = (DataManager)bf.Deserialize(stream);
            level_open = load.GetOpenLevel();
        }
        stream.Close();
    }

    public int GetOpenLevel()
    {
        return level_open;
    }
    
}
