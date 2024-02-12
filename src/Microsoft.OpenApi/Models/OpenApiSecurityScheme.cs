﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Writers;

namespace Microsoft.OpenApi.Models
{
    /// <summary>
    /// Security Scheme Object.
    /// </summary>
    public class OpenApiSecurityScheme : IOpenApiReferenceable, IOpenApiExtensible
    {
        /// <summary>
        /// REQUIRED. The type of the security scheme. Valid values are "apiKey", "http", "oauth2", "openIdConnect".
        /// </summary>
        public virtual SecuritySchemeType Type { get; set; }

        /// <summary>
        /// A short description for security scheme. CommonMark syntax MAY be used for rich text representation.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// REQUIRED. The name of the header, query or cookie parameter to be used.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// REQUIRED. The location of the API key. Valid values are "query", "header" or "cookie".
        /// </summary>
        public virtual ParameterLocation In { get; set; }

        /// <summary>
        /// REQUIRED. The name of the HTTP Authorization scheme to be used
        /// in the Authorization header as defined in RFC7235.
        /// </summary>
        public virtual string Scheme { get; set; }

        /// <summary>
        /// A hint to the client to identify how the bearer token is formatted.
        /// Bearer tokens are usually generated by an authorization server,
        /// so this information is primarily for documentation purposes.
        /// </summary>
        public virtual string BearerFormat { get; set; }

        /// <summary>
        /// REQUIRED. An object containing configuration information for the flow types supported.
        /// </summary>
        public virtual OpenApiOAuthFlows Flows { get; set; }

        /// <summary>
        /// REQUIRED. OpenId Connect URL to discover OAuth2 configuration values.
        /// </summary>
        public virtual Uri OpenIdConnectUrl { get; set; }

        /// <summary>
        /// Specification Extensions.
        /// </summary>
        public virtual IDictionary<string, IOpenApiExtension> Extensions { get; set; } = new Dictionary<string, IOpenApiExtension>();

        /// <summary>
        /// Indicates if object is populated with data or is just a reference to the data
        /// </summary>
        public bool UnresolvedReference { get; set; }

        /// <summary>
        /// Reference object.
        /// </summary>
        public OpenApiReference Reference { get; set; }

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public OpenApiSecurityScheme() { }

        /// <summary>
        /// Initializes a copy of <see cref="OpenApiSecurityScheme"/> object
        /// </summary>
        public OpenApiSecurityScheme(OpenApiSecurityScheme securityScheme)
        {
            Type = securityScheme?.Type ?? Type;
            Description = securityScheme?.Description ?? Description;
            Name = securityScheme?.Name ?? Name;
            In = securityScheme?.In ?? In;
            Scheme = securityScheme?.Scheme ?? Scheme;
            BearerFormat = securityScheme?.BearerFormat ?? BearerFormat;
            Flows = securityScheme?.Flows != null ? new(securityScheme?.Flows) : null;
            OpenIdConnectUrl = securityScheme?.OpenIdConnectUrl != null ? new Uri(securityScheme.OpenIdConnectUrl.OriginalString, UriKind.RelativeOrAbsolute) : null;
            Extensions = securityScheme?.Extensions != null ? new Dictionary<string, IOpenApiExtension>(securityScheme.Extensions) : null;
            UnresolvedReference = securityScheme?.UnresolvedReference ?? UnresolvedReference;
            Reference = securityScheme?.Reference != null ? new(securityScheme?.Reference) : null;
        }

        /// <summary>
        /// Serialize <see cref="OpenApiSecurityScheme"/> to Open Api v3.1
        /// </summary>
        public virtual void SerializeAsV31(IOpenApiWriter writer)
        {
            SerializeInternal(writer, (writer, element) => element.SerializeAsV31(writer), SerializeAsV31WithoutReference);
        }

        /// <summary>
        /// Serialize <see cref="OpenApiSecurityScheme"/> to Open Api v3.0
        /// </summary>
        public virtual void SerializeAsV3(IOpenApiWriter writer)
        {
            SerializeInternal(writer, (writer, element) => element.SerializeAsV3(writer), SerializeAsV3WithoutReference);
        }

        /// <summary>
        /// Serialize <see cref="OpenApiSecurityScheme"/> to Open Api v3.0
        /// </summary>
        private void SerializeInternal(IOpenApiWriter writer, Action<IOpenApiWriter, IOpenApiSerializable> callback,
            Action<IOpenApiWriter> action)
        {
            Utils.CheckArgumentNull(writer);;

            if (Reference != null)
            {
                callback(writer, Reference);
                return;
            }

            action(writer);
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

        internal virtual void SerializeInternalWithoutReference(IOpenApiWriter writer, OpenApiSpecVersion version, 
            Action<IOpenApiWriter, IOpenApiSerializable> callback)
        {
            writer.WriteStartObject();

            // type
            writer.WriteProperty(OpenApiConstants.Type, Type.GetDisplayName());

            // description
            writer.WriteProperty(OpenApiConstants.Description, Description);

            switch (Type)
            {
                case SecuritySchemeType.ApiKey:
                    // These properties apply to apiKey type only.
                    // name
                    // in
                    writer.WriteProperty(OpenApiConstants.Name, Name);
                    writer.WriteProperty(OpenApiConstants.In, In.GetDisplayName());
                    break;
                case SecuritySchemeType.Http:
                    // These properties apply to http type only.
                    // scheme
                    // bearerFormat
                    writer.WriteProperty(OpenApiConstants.Scheme, Scheme);
                    writer.WriteProperty(OpenApiConstants.BearerFormat, BearerFormat);
                    break;
                case SecuritySchemeType.OAuth2:
                    // This property apply to oauth2 type only.
                    // flows
                    writer.WriteOptionalObject(OpenApiConstants.Flows, Flows, callback);
                    break;
                case SecuritySchemeType.OpenIdConnect:
                    // This property apply to openIdConnect only.
                    // openIdConnectUrl
                    writer.WriteProperty(OpenApiConstants.OpenIdConnectUrl, OpenIdConnectUrl?.ToString());
                    break;
            }

            // extensions
            writer.WriteExtensions(Extensions, version);

            writer.WriteEndObject();
        }

        /// <summary>
        /// Serialize <see cref="OpenApiSecurityScheme"/> to Open Api v2.0
        /// </summary>
        public void SerializeAsV2(IOpenApiWriter writer)
        {
            Utils.CheckArgumentNull(writer);;

            if (Reference != null)
            {
                Reference.SerializeAsV2(writer);
                return;
            }

            SerializeAsV2WithoutReference(writer);
        }

        /// <summary>
        /// Serialize to OpenAPI V2 document without using reference.
        /// </summary>
        public void SerializeAsV2WithoutReference(IOpenApiWriter writer)
        {
            if (Type == SecuritySchemeType.Http && Scheme != OpenApiConstants.Basic)
            {
                // Bail because V2 does not support non-basic HTTP scheme
                writer.WriteStartObject();
                writer.WriteEndObject();
                return;
            }

            if (Type == SecuritySchemeType.OpenIdConnect)
            {
                // Bail because V2 does not support OpenIdConnect
                writer.WriteStartObject();
                writer.WriteEndObject();
                return;
            }

            writer.WriteStartObject();

            // type
            switch (Type)
            {
                case SecuritySchemeType.Http:
                    writer.WriteProperty(OpenApiConstants.Type, OpenApiConstants.Basic);
                    break;

                case SecuritySchemeType.OAuth2:
                    // These properties apply to oauth2 type only.
                    // flow
                    // authorizationUrl
                    // tokenUrl
                    // scopes
                    writer.WriteProperty(OpenApiConstants.Type, Type.GetDisplayName());
                    WriteOAuthFlowForV2(writer, Flows);
                    break;

                case SecuritySchemeType.ApiKey:
                    // These properties apply to apiKey type only.
                    // name
                    // in
                    writer.WriteProperty(OpenApiConstants.Type, Type.GetDisplayName());
                    writer.WriteProperty(OpenApiConstants.Name, Name);
                    writer.WriteProperty(OpenApiConstants.In, In.GetDisplayName());
                    break;
            }

            // description
            writer.WriteProperty(OpenApiConstants.Description, Description);

            // extensions
            writer.WriteExtensions(Extensions, OpenApiSpecVersion.OpenApi2_0);

            writer.WriteEndObject();
        }

        /// <summary>
        /// Arbitrarily chooses one <see cref="OpenApiOAuthFlow"/> object from the <see cref="OpenApiOAuthFlows"/>
        /// to populate in V2 security scheme.
        /// </summary>
        private static void WriteOAuthFlowForV2(IOpenApiWriter writer, OpenApiOAuthFlows flows)
        {
            if (flows != null)
            {
                if (flows.Implicit != null)
                {
                    WriteOAuthFlowForV2(writer, OpenApiConstants.Implicit, flows.Implicit);
                }
                else if (flows.Password != null)
                {
                    WriteOAuthFlowForV2(writer, OpenApiConstants.Password, flows.Password);
                }
                else if (flows.ClientCredentials != null)
                {
                    WriteOAuthFlowForV2(writer, OpenApiConstants.Application, flows.ClientCredentials);
                }
                else if (flows.AuthorizationCode != null)
                {
                    WriteOAuthFlowForV2(writer, OpenApiConstants.AccessCode, flows.AuthorizationCode);
                }
            }
        }

        private static void WriteOAuthFlowForV2(IOpenApiWriter writer, string flowValue, OpenApiOAuthFlow flow)
        {
            // flow
            writer.WriteProperty(OpenApiConstants.Flow, flowValue);

            // authorizationUrl
            writer.WriteProperty(OpenApiConstants.AuthorizationUrl, flow.AuthorizationUrl?.ToString());

            // tokenUrl
            writer.WriteProperty(OpenApiConstants.TokenUrl, flow.TokenUrl?.ToString());

            // scopes
            writer.WriteOptionalMap(OpenApiConstants.Scopes, flow.Scopes, (w, s) => w.WriteValue(s));
        }

        /// <summary>
        /// Parses a local file path or Url into an OpenApiSecurityScheme object.
        /// </summary>
        /// <param name="url"> The path to the OpenAPI file.</param>
        /// <param name="version">The OpenAPI specification version.</param>
        /// <param name="diagnostic"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static OpenApiSecurityScheme Load(string url, OpenApiSpecVersion version, out OpenApiDiagnostic diagnostic, OpenApiReaderSettings settings = null)
        {
            return OpenApiModelFactory.Load<OpenApiSecurityScheme>(url, version, out diagnostic, settings);
        }

        /// <summary>
        /// Reads the stream input and parses it into an OpenApiSecurityScheme object.
        /// </summary>
        /// <param name="stream">Stream containing OpenAPI description to parse.</param>
        /// <param name="format">The OpenAPI format to use during parsing.</param>
        /// <param name="version"></param>
        /// <param name="diagnostic"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static OpenApiSecurityScheme Load(Stream stream,
                                           string format,
                                           OpenApiSpecVersion version,
                                           out OpenApiDiagnostic diagnostic,
                                           OpenApiReaderSettings settings = null)
        {
            return OpenApiModelFactory.Load<OpenApiSecurityScheme>(stream, version, out diagnostic, format, settings);
        }

        /// <summary>
        /// Reads the text reader content and parses it into an OpenApiSecurityScheme object.
        /// </summary>
        /// <param name="input">TextReader containing OpenAPI description to parse.</param>
        /// <param name="format"> The OpenAPI format to use during parsing.</param>
        /// <param name="version"></param>
        /// <param name="diagnostic"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static OpenApiSecurityScheme Load(TextReader input,
                                           string format,
                                           OpenApiSpecVersion version,
                                           out OpenApiDiagnostic diagnostic,
                                           OpenApiReaderSettings settings = null)
        {
            return OpenApiModelFactory.Load<OpenApiSecurityScheme>(input, version, out diagnostic, format, settings);
        }


        /// <summary>
        /// Parses a string into a <see cref="OpenApiSecurityScheme"/> object.
        /// </summary>
        /// <param name="input"> The string input.</param>
        /// <param name="version"></param>
        /// <param name="diagnostic"></param>
        /// <param name="format"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static OpenApiSecurityScheme Parse(string input,
                                            OpenApiSpecVersion version,
                                            out OpenApiDiagnostic diagnostic,
                                            string format = null,
                                            OpenApiReaderSettings settings = null)
        {
            return OpenApiModelFactory.Parse<OpenApiSecurityScheme>(input, version, out diagnostic, format, settings);
        }
    }
}
