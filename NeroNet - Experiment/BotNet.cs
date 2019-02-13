using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public struct Node
{
    float valueNode;//тек. значение нода
    float[] weightsNode;//вес связей с пред. слоем
    
    //консутруктор нода пустышки
    public Node(byte coms)
    {
        valueNode = 0f;
        weightsNode = new float[coms];
    }
    //метод установки значения с учетом всех входящих значение в нод + веса связей
    public void SetNodeValue(float[] inputs)
    {
        float sum = 0f;
        for (int i = 0; i < inputs.Length; ++i)
            sum += inputs[i] * weightsNode[i];
        valueNode = Sigmoid(sum);
    }
    //метод установки весов нода
    public void SetNodeWeight(float _weight, int index)
    {
        weightsNode[index] = _weight;
    }
    //делать весов нода
    public float GetDeltaWeights(float error)
    {
        float delta_weight = error * (valueNode * (1 - valueNode));//производная сигмоида
        return delta_weight;
    }

    //проверочные методы для тестирования нода и сетки
    public float GetNodeValue()
    {
        return valueNode;
    }
    //получение веса нода по индексу связи
    public float GetNodeWeight(int index)
    {
        return weightsNode[index];
    }

    //sigmoid - NO CHANGE!!!!
    private float Sigmoid(float value)
    {
        return 1f / (1f + (float)System.Math.Exp(-value));
    }
}

[System.Serializable]
public struct Net
{
    private int age_net;

    //!!!В сети 1 слой!!!!
    private byte qua_input;
    private byte qua_hidden;
    private byte qua_output;
    
    private Node[] hiddenNodes;
    private Node[] outputNodes;

    //конструктор сети - пустой
    public Net(byte input,byte hidden, byte output)
    {
        age_net = 0;

        qua_input = input;
        qua_hidden = hidden;
        qua_output = output;
        
        hiddenNodes = new Node[hidden];
        outputNodes = new Node[output];

        //заполнение + рандомные веса свзяей
        {
            int r_count = input * hidden + hidden * output;
            Randomaizer random = new Randomaizer(Random.Range(0, 9999), r_count);
            for (int i = 0; i < hidden; ++i)
            {
                hiddenNodes[i] = new Node(input);
                for (int j = 0; j < input; ++j)
                    hiddenNodes[i].SetNodeWeight(random.GetRandomFloat(--r_count), j);
            }
            for (int i = 0; i < output; ++i)
            {
                outputNodes[i] = new Node(hidden);
                for (int j = 0; j < hidden; ++j)
                    outputNodes[i].SetNodeWeight(random.GetRandomFloat(--r_count), j);
            }
        }

        //чистка муссора
        System.GC.Collect();
    }
    //работа цепи
    public void NetIsWork(ref float[] data, ref float[] result)
    {
        float[] sem_result = new float[qua_hidden];
        for(int i = 0; i < qua_hidden; ++i)
        {
            hiddenNodes[i].SetNodeValue(data);
            sem_result[i] = hiddenNodes[i].GetNodeValue();
        }
        for(int i = 0; i < qua_output; ++i)
        {
            outputNodes[i].SetNodeValue(sem_result);
            result[i] = outputNodes[i].GetNodeValue();
        }
    }
    //обработка ошибки
    public void ErrorAndLearn(ref float l_rate, ref float expeted, ref float[] data)
    {
        age_net++;
        float error = 0f;
        float hidden_error = 0f;
        float n_weight = 0f;
        for(int i = 0; i < qua_output; ++i)
        {
            error = outputNodes[i].GetNodeValue() - expeted;
            for (int j = 0; j< qua_hidden; ++j)
            {
                //исправление весов выходного нода
                n_weight = outputNodes[i].GetNodeWeight(j);
                n_weight -= hiddenNodes[j].GetNodeValue() * outputNodes[i].GetDeltaWeights(error) * l_rate;
                outputNodes[i].SetNodeWeight(n_weight, j);
                //передача ошибки далее
                hidden_error = n_weight * outputNodes[i].GetDeltaWeights(error);//ошибка по тек весу пред нода
                for(int q = 0; q < qua_input; ++q)
                {
                    n_weight = hiddenNodes[j].GetNodeWeight(q);
                    n_weight -= data[q] * hiddenNodes[j].GetDeltaWeights(hidden_error) * l_rate;
                    hiddenNodes[i].SetNodeWeight(n_weight, q);
                }
            }
        }
    }
    //возраст
    public int GetAge()
    {
        return age_net;
    }

    //test
    public void WriteAllWeightsNodes()
    {
        for(int i = 0; i < qua_hidden; ++i)
        {
            Debug.Log("Hidden Node - " + i);
            for (int j = 0; j < qua_input; ++j)
                Debug.Log("Weight - " + j + " = " + hiddenNodes[i].GetNodeWeight(j));
        }
        for (int i = 0; i < qua_output; ++i)
        {
            Debug.Log("Output Node - " + i);
            for (int j = 0; j < qua_hidden; ++j)
                Debug.Log("Weight - " + j + " = " + outputNodes[i].GetNodeWeight(j));
        }
    }
}

//рандомайзер
public struct Randomaizer
{
    System.Random random;
    float[] randomNumbs;
    //конструктор
    public Randomaizer(int seed, int count)
    {
        random = new System.Random(seed);
        randomNumbs = new float[count];
        for (int i = 0; i < count; ++i)
            randomNumbs[i] = (float) random.NextDouble();
    }
    //передатчик
    public float GetRandomFloat(int index)
    {
        return randomNumbs[index];
    }
}

public class BotNet : MonoBehaviour
{
    private Net net_1;
    public Bot bot;
    public TestTarget target;
    public int net_learn_age;

    public float learning_rate;
    public float expeted_net;

    public float[] test_value;

    public float[] callBackNet;
    [Header("Simulation")]
    public float sim_time;
    private bool sim_is_go;

    private void Start()
    {
        test_value = new float[4];
        callBackNet = new float[1];
        if(!File.Exists(Application.streamingAssetsPath + "/" + "bot_net" + ".td"))
        {
            print("Create New NET!");
            net_1 = new Net(4, 2, 1);
        }
        else
        {
            print("Load NET!");
            LoadBotNet();
        }
        net_learn_age = net_1.GetAge();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            SaveBotNet();
        if (Input.GetKeyDown(KeyCode.F))
            net_1.WriteAllWeightsNodes();
        if (Input.GetKeyDown(KeyCode.W))
            net_1.NetIsWork(ref test_value, ref callBackNet);
        if (Input.GetKeyDown(KeyCode.R))
        {
            net_1.ErrorAndLearn(ref learning_rate, ref expeted_net, ref test_value);
            net_learn_age++;
        }
        //ручной пуск рандомности
        if (Input.GetKeyDown(KeyCode.R))
            RandomSpawnCase();
    }

    private void RandomSpawnCase()
    {
        //рандомное распределение цели в заданном квадрате + характеристики
        {
            target.transform.localPosition = new Vector3(Random.Range(-50f, 50f), 0f, Random.Range(-50f, 50f));
            target.transform.localRotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
        }
        //рандомный поворот бота, но расположение в zero
        {
            bot.transform.localPosition = Vector3.zero;
            bot.transform.localRotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
        }
    }

    //save - load
    private void SaveBotNet()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.streamingAssetsPath + "/" + "bot_net" + ".td", FileMode.Create);
        bf.Serialize(stream, net_1);
        stream.Close();
    }

    private void LoadBotNet()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.streamingAssetsPath + "/" + "bot_net" + ".td", FileMode.Open);
        net_1 = (Net) bf.Deserialize(stream);
        stream.Close();
    }
    
}
