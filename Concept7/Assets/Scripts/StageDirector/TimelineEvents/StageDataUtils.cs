using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

public static class StageDataUtils
{
    // deserialize YAML, and generate a useful error if it fails.
    public static T Deserialize<T>(StageData.Actor actor, string section, string yaml)
    {
        try
        {
            return StageData.Deserializer.Deserialize<T>(yaml);
        }
        catch (Exception e)
        {
            IStageDataErrorThrower result = CheckSchema(typeof(T), yaml, StageData.Deserializer, StageData.Serializer, "");
            if (result != null)
            {
                result.Throw(actor, section, yaml);
            }
            throw e;
        }
    }
    // recursively check the YAML against the type we're trying to deserialize.
    // I wish YamlDotNet supported this instead of just erroring.
    static IStageDataErrorThrower CheckSchema(Type t, string yaml, IDeserializer deserializer, ISerializer serializer, string field)
    {
        // primitive type
        if (t.IsPrimitive || t == typeof(decimal) || t == typeof(string))
        {
            try
            {
                deserializer.Deserialize(yaml, t);
            }
            catch (FormatException)
            {
                return new StageDataTypeError() { Field = field, Type = t.Name.ToLower(), Yaml = yaml };
            }
            catch (YamlException)
            {
                return new StageDataTypeError() { Field = field, Type = t.Name.ToLower(), Yaml = yaml };
            }
        }
        // check this nullable, if it is one
        else if (t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
        {
            Type subtype = t.GetGenericArguments()[0];
            IStageDataErrorThrower r = CheckSchema(subtype, yaml, deserializer, serializer, field);
            if (r != null)
            {
                return r;
            }
        }
        // recursively check this list, if it is one
        else if (t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
        {
            List<object> seq;
            try
            {
                seq = deserializer.Deserialize<List<object>>(yaml);
            }
            catch (FormatException)
            {
                return new StageDataTypeError() { Field = field, Type = "list", Yaml = yaml };
            }
            catch (YamlException)
            {
                return new StageDataTypeError() { Field = field, Type = "list", Yaml = yaml };
            }
            Type subtype = t.GetGenericArguments()[0];
            for (int i = 0; i < seq.Count; i++)
            {
                IStageDataErrorThrower r = CheckSchema(subtype, serializer.Serialize(seq[i]), deserializer, serializer, $"{field}.[{i}]");
                if (r != null)
                {
                    return r;
                }
            }
        }
        // recursively check this dict, if it is one
        else if (t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
        {
            Dictionary<string, object> map;
            try
            {
                map = deserializer.Deserialize<Dictionary<string, object>>(yaml);
            }
            catch (FormatException)
            {
                return new StageDataTypeError() { Field = field, Type = "dict", Yaml = yaml };
            }
            catch (YamlException)
            {
                return new StageDataTypeError() { Field = field, Type = "dict", Yaml = yaml };
            }
            Type subtype = t.GetGenericArguments()[1];
            foreach (string s in map.Keys)
            {
                IStageDataErrorThrower r = CheckSchema(subtype, serializer.Serialize(map[s]), deserializer, serializer, $"{field}.{s}");
                if (r != null)
                {
                    return r;
                }
            }
        }
        // recursively check this object/struct
        else if (!t.IsGenericType)
        {
            Dictionary<string, Type> schema = new Dictionary<string, Type>();
            foreach (FieldInfo f in t.GetFields())
            {
                schema[f.Name] = f.FieldType;
            }
            Dictionary<string, object> data;
            try
            {
                data = deserializer.Deserialize<Dictionary<string, object>>(yaml);
            }
            catch (FormatException)
            {
                return new StageDataTypeError() { Field = field, Type = "dict", Yaml = yaml };
            }
            catch (YamlException)
            {
                return new StageDataTypeError() { Field = field, Type = "dict", Yaml = yaml };
            }
            foreach (string s in data.Keys)
            {
                string fieldname = ToCamelCase(s);
                if (!schema.ContainsKey(fieldname))
                {
                    return new StageDataFieldNotFoundError() { FieldName = s, Field = $"{field}.{s}", AllowedFields = schema.Keys.Select(x => ToUnderscoreCase(x)).ToList(), Yaml = yaml };
                }
                IStageDataErrorThrower r = CheckSchema(schema[fieldname], serializer.Serialize(data[s]), deserializer, serializer, $"{field}.{s}");
                if (r != null)
                {
                    return r;
                }
            }
        }
        else
        {
            throw new StageDataException($"Unrecognized generic {t}");
        }
        // no error
        return null;
    }
    // convert between camel case fields (C#) and underscore case fields (YAML)
    public static string ToCamelCase(string str)
    {
        return str.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries).Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1)).Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }
    public static string ToUnderscoreCase(string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }
    interface IStageDataErrorThrower
    {
        void Throw(StageData.Actor actor, string section, string totalYaml);
    }
    class StageDataTypeError : IStageDataErrorThrower
    {
        public string Field;
        public string Type;
        public string Yaml;

        public void Throw(StageData.Actor actor, string section, string totalYaml)
        {
            string actorDesc = actor == null ? $"{section} is" : $"{section} in actor {actor.Name} in file {actor.File} has a field";
            string fieldOrVal = string.IsNullOrEmpty(Field) ? "Value" : $"Field '{Field}'";
            // don't care about precision currently
            if (Type == "single" || Type == "double")
            {
                Type = "float";
            }
            throw new StageDataException($"{actorDesc} of incorrect type.{Environment.NewLine}" +
                $"{fieldOrVal} must be of type {Type}.{Environment.NewLine}" +
                $"The offending YAML is shown below:{Environment.NewLine}{Environment.NewLine}{totalYaml}");
        }
    }
    class StageDataFieldNotFoundError : IStageDataErrorThrower
    {
        public string Field;
        public string FieldName;
        public List<string> AllowedFields;
        public string Yaml;

        public void Throw(StageData.Actor actor, string section, string totalYaml)
        {
            string actorDesc = actor == null ? $"{section}" : $"{section} in actor {actor.Name} in file {actor.File}";
            throw new StageDataException($"{actorDesc} attempts to use a field which doesn't exist at location {Field}.{Environment.NewLine}" +
                $"'{FieldName}' is not recognized. Only the following fields can be used: [{string.Join(", ", AllowedFields)}].{Environment.NewLine}" +
                $"The offending YAML is shown below:{Environment.NewLine}{Environment.NewLine}{totalYaml}");
        }
    }

    // Convert X/Y/Dir/Dist/Rel and a current pos/dir into a final position.
    // Dist cannot be null.
    public static Vector2 FindDestPosition(float? X, float? Y, float? Dir, float? Dist, string Rel, Vector2 pos, Vector2 dir)
    {
        string rel = Rel ?? "pos";
        // relative position, absolute rotation
        if (rel == "pos")
        {
            if (X != null || Y != null)
            {
                return new Vector2(pos.x + (X ?? 0), pos.y + (Y ?? 0));
            }
            else if (Dir != null)
            {
                return pos + (Vector2)(Quaternion.Euler(0, 0, Dir.Value) * Vector2.right * Dist.Value);
            }
            return pos;
        }
        // set absolute (world) position
        if (rel == "abs")
        {
            return new Vector2(X ?? pos.x, Y ?? pos.y);
        }
        // relative to dir when command began
        if (rel == "dir")
        {
            if (X != null || Y != null)
            {
                Vector2 diff = new Vector2(X ?? 0, Y ?? 0);
                return pos + (Vector2)(Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x)) * diff);
            }
            else if (Dir != null)
            {
                Vector2 diff = Quaternion.Euler(0, 0, Dir.Value) * Vector2.right * Dist.Value;
                return pos + (Vector2)(Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x)) * diff);
            }
            return pos;
        }
        throw new StageDataException($"Invalid rel value: {rel}, allowed values are ['abs', 'pos', 'dir']");
    }

    // Get or create a parent for an actor.
    public static GameObject GetParent(string parenting, Vector2 pos, GameObject actor, string emitter=null)
    {
        // if set no parent
        if (parenting == null)
        {
            return null;
        }
        if (parenting == "new")
        {
            actor = UnityEngine.Object.Instantiate(StageDirector.Instance.Prefabs["ActorGroup"]);
            actor.transform.position = new Vector3(pos.x, pos.y, actor.transform.position.z);
            return actor;
        }
        if (parenting == "actor")
        {
            return actor;
        }
        if (parenting == "emitter")
        {
            return actor.GetComponent<StageActor>().Emitters[emitter];
        }
        throw new StageDataException($"Invalid parent value: {parenting}, allowed values are [null, 'new', 'actor', 'emitter']");
    }

    // get variable, defaulting to zero if it doesn't exist.
    public static float GetVar(StageActor actor, string s)
    {
        if (s == null)
        {
            return 0f;
        }
        return actor.Vars[s];
    }
}
