using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace LUPLoader
{
    public class DiskQueue<T>
    {
        string VariableName = "";
        object ActLock = new object();
        List<T> data = new List<T>();
        public DiskQueue(string variableName)
        {
            VariableName = variableName;
        }
        public void Enqueue(T v)
        {
            lock (ActLock)
            {
                Deserialize();
                data.Add(v);
                Serialize();
            }
        }
        public T Dequeue()
        {
            lock (ActLock)
            {
                Deserialize();
                if (data.Count > 0)
                {
                    var d = data[0];
                    data.RemoveAt(0);
                    Serialize();
                    return d;
                }
                throw new Exception("Очередь пуста");
            }
        }

        public bool TryDequeue(out T v)
        {
            v = default(T);
            lock (ActLock)
            {
                Deserialize();
                if (data.Count > 0)
                {
                    var d = data[0];
                    data.RemoveAt(0);
                    Serialize();
                    v = d;
                    return true;
                }
                return false;
            }
        }

        public void ReplaceFirst(T v)
        {
            lock (ActLock)
            {
                Deserialize();
                if (data.Count > 0)
                {
                    data.RemoveAt(0);
                }
                data.Insert(0, v);
                Serialize();
            }
        }

        public bool TryPeek(out T v)
        {
            v = default(T);
            lock (ActLock)
            {
                Deserialize();
                if (data.Count > 0)
                {
                    v = data[0];
                    //v = d;
                    return true;
                }
                return false;
            }
        }

        public List<T> ToList()
        {
            var dt = new List<T>();
            dt.AddRange(data);
            return dt;
        }

        public int Count
        {
            get
            {
                lock (ActLock)
                {
                    Deserialize();
                    return data.Count;
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (ActLock)
                {
                    Deserialize();
                    return data.Count == 0;
                }
            }
        }
        protected string GetExePath()
        {
            var path = System.Reflection.Assembly.GetEntryAssembly().Location;
            path = Path.GetDirectoryName(path);
            if (!path.EndsWith("\\")) path = path + "\\";
            return path;
        }

        public void Serialize()
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(data.GetType());

            using (FileStream fs = new FileStream(GetExePath() + VariableName + ".json", FileMode.Create))
            {
                jsonFormatter.WriteObject(fs, data);
            }


        }
        public void Deserialize()
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(data.GetType());
            var fn = GetExePath() + VariableName + ".json";
            if (File.Exists(fn))
            {

                using (FileStream fs = new FileStream(fn, FileMode.Open))
                {
                    data = (List<T>)jsonFormatter.ReadObject(fs);

                }
            }
        }


    }

}
