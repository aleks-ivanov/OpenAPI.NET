﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers.Exceptions;
//using SharpYaml.Schemas;
//using SharpYaml.Serialization;

namespace Microsoft.OpenApi.Readers.ParseNodes
{
    /// <summary>
    /// Abstraction of a Map to isolate semantic parsing from details of JSON DOM
    /// </summary>
    internal class MapNode : ParseNode, IEnumerable<PropertyNode>
    {
        private readonly JsonObject _node;
        private readonly List<PropertyNode> _nodes;

        public MapNode(ParsingContext context, string jsonString) :
            this(context, YamlHelper.ParseJsonString(jsonString))
        {
        }
        public MapNode(ParsingContext context, JsonNode node) : base(
            context)
        {
            if (!(node is JsonObject mapNode))
            {
                throw new OpenApiReaderException("Expected map.", Context);
            }

            //_node = mapNode;
            _nodes = _node.Select(p => new PropertyNode(Context, p.Key, p.Value)).ToList();

            //_nodes = this._node.Children
            //    .Select(kvp => new PropertyNode(Context, kvp.Key.GetScalarValue(), kvp.Value))
            //    .Cast<PropertyNode>()
            //    .ToList();
        }

        public PropertyNode this[string key]
        {
            get
            {
                if (_node.TryGetPropertyValue(key, out var node))
                {
                    return new PropertyNode(Context, key, node);
                }

                return null;
            }
        }

        public override Dictionary<string, T> CreateMap<T>(Func<MapNode, T> map)
        {
            var jsonMap = _node ?? throw new OpenApiReaderException($"Expected map while parsing {typeof(T).Name}", Context);
            var nodes = jsonMap.Select(
                n => {
                    
                    var key = n.Key;
                    T value;
                    try
                    {
                        Context.StartObject(key);
                        value = n.Value as JsonObject == null
                          ? default
                          : map(new MapNode(Context, n.Value as JsonObject));
                    } 
                    finally
                    {
                        Context.EndObject();
                    }
                    return new
                    {
                        key,
                        value
                    };
                });

            return nodes.ToDictionary(k => k.key, v => v.value);
        }

        public override Dictionary<string, T> CreateMapWithReference<T>(
            ReferenceType referenceType,
            Func<MapNode, T> map) 
        {
            var jsonMap = _node ?? throw new OpenApiReaderException($"Expected map while parsing {typeof(T).Name}", Context);            

            var nodes = jsonMap.Select(
                n =>
                {
                    var key = n.Key;
                    (string key, T value) entry;
                    try
                    {
                        Context.StartObject(key);
                        entry = (key,
                            value: map(new MapNode(Context, (JsonObject)n.Value))
                        );
                        if (entry.value == null)
                        {
                            return default;  // Body Parameters shouldn't be converted to Parameters
                        }
                        // If the component isn't a reference to another component, then point it to itself.
                        if (entry.value.Reference == null)
                        {
                            entry.value.Reference = new OpenApiReference()
                            {
                                Type = referenceType,
                                Id = entry.key
                            };
                        }
                     }
                    finally
                    {
                        Context.EndObject();
                    }
                    return entry;
                }
                );
            return nodes.Where(n => n != default).ToDictionary(k => k.key, v => v.value);
        }

        public override Dictionary<string, T> CreateSimpleMap<T>(Func<ValueNode, T> map)
        {
            var jsonMap = _node ?? throw new OpenApiReaderException($"Expected map while parsing {typeof(T).Name}", Context);
            var nodes = jsonMap.Select(
                n =>
                {
                    var key = n.Key;
                    try
                    {
                        Context.StartObject(key);
                        JsonValue valueNode = n.Value as JsonValue;
                        
                        if (valueNode == null)
                        {
                            throw new OpenApiReaderException($"Expected scalar while parsing {typeof(T).Name}", Context);
                        }
                        
                        return (key, value: map(new ValueNode(Context, (JsonValue)n.Value)));
                    } finally {
                        Context.EndObject();
                    }
                });
            
            return nodes.ToDictionary(k => k.key, v => v.value);
        }

        public IEnumerator<PropertyNode> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        public override string GetRaw()
        {
            var x = JsonSerializer.Serialize(_node); // (new SerializerSettings(new JsonSchema()) { EmitJsonComptible = true });
            return x;
        }

        public T GetReferencedObject<T>(ReferenceType referenceType, string referenceId, string summary = null, string description = null)
            where T : IOpenApiReferenceable, new()
        {
            return new T()
            {
                UnresolvedReference = true,
                Reference = Context.VersionService.ConvertToOpenApiReference(referenceId, referenceType, summary, description)
            };
        }

        public string GetReferencePointer()
        {
            if (!_node.TryGetPropertyValue("$ref", out JsonNode refNode))
            {
                return null;
            }

            return refNode.GetScalarValue();
        }

        public string GetScalarValue(ValueNode key)
        {
            var scalarNode = _node[key.GetScalarValue()] as JsonValue;
            if (scalarNode == null)
            {
                //throw new OpenApiReaderException($"Expected scalar at line {_node.Start.Line} for key {key.GetScalarValue()}", Context);
            }

            return scalarNode.GetValue<string>();
        }

        /// <summary>
        /// Create a <see cref="OpenApiObject"/>
        /// </summary>
        /// <returns>The created Any object.</returns>
        public override IOpenApiAny CreateAny()
        {
            var apiObject = new OpenApiObject();
            foreach (var node in this)
            {
                apiObject.Add(node.Name, node.Value.CreateAny());
            }

            return apiObject;
        }
    }
}
