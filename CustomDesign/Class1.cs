﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomDesign
{
    public class CustomDesign
    {
        public Observe Observe = new Observe();

        Stack<CustomType> TypeStack = new Stack<CustomType>();

        public bool LoadJson(string data)
        {
            JArray list = JArray.Parse(data);

            for (int i = 0; i < list.Count; i++)
            {
                var Type = Observe[list[i]["Name"].ToString()];
                var Enum = list[i].Children();
                TypeStack.Push(Type);
                foreach (var token in Enum)
                {
                    SelectCode(token);
                }
            }
            return true;
        }

        JToken SelectCode(JToken token)
        {
            JProperty p = token.ToObject<JProperty>();
            if (p.Name == "Field")
            {
                foreach (var to in token.Children().Children())
                {
                    CustomType type = TypeStack.Peek();
                    var t = GetField(to, type);
                    TypeStack.Push(t.Item2);
                    foreach (var to2 in to)
                    {
                        SelectCode(to2);
                    }
                    TypeStack.Pop();
                }

            }
            else if (p.Name == "Property")
            {
                foreach (var to in token.Children().Children())
                {
                    CustomType type = TypeStack.Peek();
                    var t = GetProperty(to, type);
                    TypeStack.Push(t.Item2);
                    foreach (var to2 in to)
                    {
                        SelectCode(to2);
                    }
                    TypeStack.Pop();
                }
            }
            else if (p.Name == "Type")
            {
                var type = Type.GetType(p.Value.ToString());
                token = token.Next;
                var data = token.ToObject<JProperty>();
                if (data.Name == "Value")
                {
                    var t = TypeStack.Pop();
                    t.Field?.SetValue(t.Value, Convert.ChangeType(data.Value, type));
                    t.Property?.SetValue(TypeStack.Peek().Value, Convert.ChangeType(data.Value, type));
                    TypeStack.Push(t);
                }
            }
            return token;
        }

        public (JToken, CustomType) GetField(JToken token, CustomType type)
        {
            var name = token["Name"].ToString();
            var tmp = Observe.GetField(type, name, BindingFlags.NonPublic | BindingFlags.Instance);
            CustomType w = new CustomType(tmp.Item1, tmp.Item2.Value, type.Name + name);
            foreach (var t in token.Children())
            {
                return (t.First, w);
            }
            return (null, w);
        }

        public (JToken, CustomType) GetProperty(JToken token, CustomType type)
        {
            var name = token["Name"].ToString();
            var tmp = Observe.GetProperty(type, name, BindingFlags.Public | BindingFlags.Instance);
            CustomType w = new CustomType(tmp.Item1, tmp.Item2.Value, type.Name + name);
            foreach (var t in token.Children())
            {
                return (t.First, w);
            }
            return (null, w);
        }

    }
}
