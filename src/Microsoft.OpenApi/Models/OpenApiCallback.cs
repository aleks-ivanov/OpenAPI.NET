﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Writers;

namespace Microsoft.OpenApi.Models
{
    /// <summary>
    /// Callback Object: A map of possible out-of band callbacks related to the parent operation.
    /// </summary>
    public class OpenApiCallback : IOpenApiReferenceable, IOpenApiExtensible, IEffective<OpenApiCallback>
    {
        /// <summary>
        /// A Path Item Object used to define a callback request and expected responses.
        /// </summary>
        public virtual Dictionary<RuntimeExpression, OpenApiPathItem> PathItems { get; set; }
            = new();

        /// <summary>
        /// Indicates if object is populated with data or is just a reference to the data
        /// </summary>
        public virtual bool UnresolvedReference { get; set; }

        /// <summary>
        /// Reference pointer.
        /// </summary>
        public OpenApiReference Reference { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public virtual IDictionary<string, IOpenApiExtension> Extensions { get; set; } = new Dictionary<string, IOpenApiExtension>();

        /// <summary>
        /// Parameter-less constructor
        /// </summary>
        public OpenApiCallback() { }

        /// <summary>
        /// Initializes a copy of an <see cref="OpenApiCallback"/> object
        /// </summary>
        public OpenApiCallback(OpenApiCallback callback)
        {
            PathItems = callback?.PathItems != null ? new(callback?.PathItems) : null;
            UnresolvedReference = callback?.UnresolvedReference ?? UnresolvedReference;
            Reference = callback?.Reference != null ? new(callback?.Reference) : null;
            Extensions = callback?.Extensions != null ? new Dictionary<string, IOpenApiExtension>(callback.Extensions) : null;
        }

        /// <summary>
        /// Add a <see cref="OpenApiPathItem"/> into the <see cref="PathItems"/>.
        /// </summary>
        /// <param name="expression">The runtime expression.</param>
        /// <param name="pathItem">The path item.</param>
        public void AddPathItem(RuntimeExpression expression, OpenApiPathItem pathItem)
        {
            Utils.CheckArgumentNull(expression);
            Utils.CheckArgumentNull(pathItem);

            if (PathItems == null)
            {
                PathItems = new();
            }

            PathItems.Add(expression, pathItem);
        }

        /// <summary>
        /// Serialize <see cref="OpenApiCallback"/> to Open Api v3.1
        /// </summary>
        /// <param name="writer"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void SerializeAsV31(IOpenApiWriter writer)
        {
            SerializeInternal(writer, (writer, element) => element.SerializeAsV31(writer),
                (writer, referenceElement) => referenceElement.SerializeAsV31WithoutReference(writer));
        }

        /// <summary>
        /// Serialize <see cref="OpenApiCallback"/> to Open Api v3.0
        /// </summary>
        public virtual void SerializeAsV3(IOpenApiWriter writer)
        {
            SerializeInternal(writer, (writer, element) => element.SerializeAsV3(writer),
                (writer, referenceElement) => referenceElement.SerializeAsV3WithoutReference(writer));
        }

        /// <summary>
        /// Serialize <see cref="OpenApiCallback"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="callback"></param>
        /// <param name="action"></param>
        private void SerializeInternal(IOpenApiWriter writer,
            Action<IOpenApiWriter, IOpenApiSerializable> callback,
            Action<IOpenApiWriter, IOpenApiReferenceable> action)
        {
            Utils.CheckArgumentNull(writer);

            var target = this;
            
            if (Reference != null)
            {
                if (!writer.GetSettings().ShouldInlineReference(Reference))
                {
                    callback(writer, Reference);
                    return;
                }
                else
                {
                    target = GetEffective(Reference.HostDocument);
                }
            }
            
            action(writer, target);
        }

        /// <summary>
        /// Returns an effective OpenApiCallback object based on the presence of a $ref
        /// </summary>
        /// <param name="doc">The host OpenApiDocument that contains the reference.</param>
        /// <returns>OpenApiCallback</returns>
        public OpenApiCallback GetEffective(OpenApiDocument doc)
        {
            if (Reference != null)
            {
                return doc.ResolveReferenceTo<OpenApiCallback>(Reference);
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Serialize to OpenAPI V31 document without using reference.
        /// </summary>
        public virtual void SerializeAsV31WithoutReference(IOpenApiWriter writer)
        {
            SerializeInternalWithoutReference(writer, OpenApiSpecVersion.OpenApi3_1,
                (writer, element) => element.SerializeAsV31(writer));
        }

        /// <summary>
        /// Serialize to OpenAPI V3 document without using reference.
        /// </summary>
        public virtual void SerializeAsV3WithoutReference(IOpenApiWriter writer)
        {
            SerializeInternalWithoutReference(writer, OpenApiSpecVersion.OpenApi3_0,
                (writer, element) => element.SerializeAsV3(writer));
        }

        internal void SerializeInternalWithoutReference(IOpenApiWriter writer, OpenApiSpecVersion version,
            Action<IOpenApiWriter, IOpenApiSerializable> callback)
        {
            writer.WriteStartObject();

            // path items
            foreach (var item in PathItems)
            {
                writer.WriteRequiredObject(item.Key.Expression, item.Value, callback);
            }

            // extensions
            writer.WriteExtensions(Extensions, version);

            writer.WriteEndObject();
        }

        /// <summary>
        /// Serialize <see cref="OpenApiCallback"/> to Open Api v2.0
        /// </summary>
        public void SerializeAsV2(IOpenApiWriter writer)
        {
            // Callback object does not exist in V2.
        }

        /// <summary>
        /// Serialize to OpenAPI V2 document without using reference.
        /// </summary>

        public void SerializeAsV2WithoutReference(IOpenApiWriter writer)
        {
            // Callback object does not exist in V2.
        }

        /// <summary>
        /// Parses a local file path or Url into an Open API document.
        /// </summary>
        /// <param name="url"> The path to the OpenAPI file.</param>
        /// <param name="version">The OpenAPI specification version.</param>
        /// <param name="diagnostic"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static OpenApiCallback Load(string url, OpenApiSpecVersion version, out OpenApiDiagnostic diagnostic, OpenApiReaderSettings settings = null)
        {
            return OpenApiModelFactory.Load<OpenApiCallback>(url, version, out diagnostic, settings);
        }

        /// <summary>
        /// Reads the stream input and parses it into an Open API document.
        /// </summary>
        /// <param name="stream">Stream containing OpenAPI description to parse.</param>
        /// <param name="format">The OpenAPI format to use during parsing.</param>
        /// <param name="version"></param>
        /// <param name="diagnostic"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static OpenApiCallback Load(Stream stream,
                                           string format,
                                           OpenApiSpecVersion version,
                                           out OpenApiDiagnostic diagnostic,
                                           OpenApiReaderSettings settings = null)
        {
            return OpenApiModelFactory.Load<OpenApiCallback>(stream, version, out diagnostic, format, settings);
        }

        /// <summary>
        /// Reads the text reader content and parses it into an Open API document.
        /// </summary>
        /// <param name="input">TextReader containing OpenAPI description to parse.</param>
        /// <param name="format"> The OpenAPI format to use during parsing.</param>
        /// <param name="version"></param>
        /// <param name="diagnostic"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static OpenApiCallback Load(TextReader input,
                                           string format,
                                           OpenApiSpecVersion version,
                                           out OpenApiDiagnostic diagnostic,
                                           OpenApiReaderSettings settings = null)
        {
            return OpenApiModelFactory.Load<OpenApiCallback>(input, version, out diagnostic, format, settings);
        }


        /// <summary>
        /// Parses a string into a <see cref="OpenApiDocument"/> object.
        /// </summary>
        /// <param name="input"> The string input.</param>
        /// <param name="version"></param>
        /// <param name="diagnostic"></param>
        /// <param name="format"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static OpenApiCallback Parse(string input,
                                            OpenApiSpecVersion version,
                                            out OpenApiDiagnostic diagnostic,
                                            string format = null,
                                            OpenApiReaderSettings settings = null)
        {
            return OpenApiModelFactory.Parse<OpenApiCallback>(input, version, out diagnostic, format, settings);
        }
    }
}
