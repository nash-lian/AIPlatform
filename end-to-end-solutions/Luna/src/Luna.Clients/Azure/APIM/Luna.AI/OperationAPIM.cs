﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients.Azure.APIM
{
    public class OperationAPIM : IOperationAPIM
    {
        private const string REQUEST_BASE_URL_FORMAT = "https://{0}.management.azure-api.net";
        private string PATH_FORMAT = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/apis/{3}/operations/{4}";
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private string _apiVersion;
        private APIMAuthHelper _apimAuthHelper;
        private HttpClient _httpClient;

        private string _requestBaseUrl;

        [ActivatorUtilitiesConstructor]
        public OperationAPIM(IOptionsMonitor<APIMConfigurationOption> options,
                           HttpClient httpClient,
                           IKeyVaultHelper keyVaultHelper)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _subscriptionId = options.CurrentValue.Config.SubscriptionId;
            _resourceGroupName = options.CurrentValue.Config.ResourceGroupname;
            _apimServiceName = options.CurrentValue.Config.APIMServiceName;
            _apiVersion = options.CurrentValue.Config.APIVersion;
            _requestBaseUrl = string.Format(REQUEST_BASE_URL_FORMAT, _apimServiceName);
            _apimAuthHelper = new APIMAuthHelper(options.CurrentValue.Config.UId, keyVaultHelper.GetSecretAsync(options.CurrentValue.Config.VaultName, options.CurrentValue.Config.Key).Result);
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        private Models.Azure.Operation RealTimePrediction()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "real-time-prediction";
            operation.properties.displayName = "real-time-prediction";
            operation.properties.method = "POST";
            operation.properties.urlTemplate = "/predict";

            return operation;
        }

        private Models.Azure.Operation BatchInferenceWithDefaultModel()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "batch-inference-with-default-model";
            operation.properties.displayName = "batch-inference-with-default-model";
            operation.properties.method = "POST";
            operation.properties.urlTemplate = "/batchinference";

            return operation;
        }

        private Models.Azure.Operation GetABatchInferenceOperationWithDefaultModel()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "get-a-batch-inference-operation-with-default-model";
            operation.properties.displayName = "get-a-batch-inference-operation-with-default-model";
            operation.properties.method = "GET";
            operation.properties.urlTemplate = "/operations/{operationId}";

            return operation;
        }

        private Models.Azure.Operation GetAllBatchInferenceOperationsWithDefaultModel()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "get-all-batch-inference-operations-with-default-model";
            operation.properties.displayName = "get-all-batch-inference-operations-with-default-model";
            operation.properties.method = "GET";
            operation.properties.urlTemplate = "/operations";

            return operation;
        }

        private Models.Azure.Operation TrainModel()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "train-model";
            operation.properties.displayName = "train-model";
            operation.properties.method = "POST";
            operation.properties.urlTemplate = "/train";

            return operation;
        }

        private Models.Azure.Operation GetAModel()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "get-a-model";
            operation.properties.displayName = "get-a-model";
            operation.properties.method = "GET";
            operation.properties.urlTemplate = "/models/{modelId}";

            return operation;
        }

        private Models.Azure.Operation GetAllModels()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "get-all-models";
            operation.properties.displayName = "get-all-models";
            operation.properties.method = "GET";
            operation.properties.urlTemplate = "/models";

            return operation;
        }

        private Models.Azure.Operation BatchInference()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "batch-inference";
            operation.properties.displayName = "batch-inference";
            operation.properties.method = "POST";
            operation.properties.urlTemplate = "/models/{modelId}/batchinference";

            return operation;
        }

        private Models.Azure.Operation GetABatchInferenceOperation()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "get-a-batch-inference-operation";
            operation.properties.displayName = "get-a-batch-inference-operation";
            operation.properties.method = "GET";
            operation.properties.urlTemplate = "/operations/{operationId}";

            return operation;
        }

        private Models.Azure.Operation GetAllBatchInferenceOperations()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "get-all-batch-inference-operations";
            operation.properties.displayName = "get-all-batch-inference-operations";
            operation.properties.method = "GET";
            operation.properties.urlTemplate = "/operations";

            return operation;
        }

        private Models.Azure.Operation DeployRealTimePredictionEndpoint()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "deploy-real-time-prediction-endpoint";
            operation.properties.displayName = "deploy-real-time-prediction-endpoint";
            operation.properties.method = "POST";
            operation.properties.urlTemplate = "/models/{modelId}/deploy";

            return operation;
        }

        private Models.Azure.Operation GetADeployedEndpoint()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "get-a-deployed-endpoint";
            operation.properties.displayName = "get-a-deployed-endpoint";
            operation.properties.method = "GET";
            operation.properties.urlTemplate = "/endpoints/{deploymentId}";

            return operation;
        }

        private Models.Azure.Operation GetAllDeployedEndpoints()
        {
            Models.Azure.Operation operation = new Models.Azure.Operation();

            operation.name = "get-all-deployed-endpoints";
            operation.properties.displayName = "get-all-deployed-endpoints";
            operation.properties.method = "GET";
            operation.properties.urlTemplate = "/endpoints";

            return operation;
        }



        private Uri GetAPIVersionAPIMRequestURI(string productName, string deploymentName, string versionName, string operationName, IDictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder(_requestBaseUrl + GetAPIMRESTAPIPath(productName, deploymentName, versionName, operationName));

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in queryParams ?? new Dictionary<string, string>()) query[kv.Key] = kv.Value;
            query["api-version"] = _apiVersion;
            string queryString = query.ToString();

            builder.Query = query.ToString();

            return new Uri(builder.ToString());
        }

        public string GetAPIMRESTAPIPath(string productName, string deploymentName, string versionName, string operationName)
        {
            return string.Format(PATH_FORMAT, _subscriptionId, _resourceGroupName, _apimServiceName, productName + deploymentName + versionName, operationName);
        }

        public Models.Azure.Operation GetOperation(Models.Azure.OperationTypeEnum operationType)
        {
            switch (operationType)
            {
                case Models.Azure.OperationTypeEnum.RealTimePrediction:
                    return RealTimePrediction();
                case Models.Azure.OperationTypeEnum.BatchInferenceWithDefaultModel:
                    return BatchInferenceWithDefaultModel();
                case Models.Azure.OperationTypeEnum.GetABatchInferenceOperationWithDefaultModel:
                    return GetABatchInferenceOperationWithDefaultModel();
                case Models.Azure.OperationTypeEnum.GetAllBatchInferenceOperationsWithDefaultModel:
                    return GetAllBatchInferenceOperationsWithDefaultModel();
                case Models.Azure.OperationTypeEnum.TrainModel:
                    return TrainModel();
                case Models.Azure.OperationTypeEnum.GetAModel:
                    return GetAModel();
                case Models.Azure.OperationTypeEnum.GetAllModels:
                    return GetAllModels();
                case Models.Azure.OperationTypeEnum.BatchInference:
                    return BatchInference();
                case Models.Azure.OperationTypeEnum.GetABatchInferenceOperation:
                    return GetABatchInferenceOperation();
                case Models.Azure.OperationTypeEnum.GetAllBatchInferenceOperations:
                    return GetAllBatchInferenceOperations();
                case Models.Azure.OperationTypeEnum.DeployRealTimePredictionEndpoint:
                    return DeployRealTimePredictionEndpoint();
                case Models.Azure.OperationTypeEnum.GetADeployedEndpoint:
                    return GetADeployedEndpoint();
                case Models.Azure.OperationTypeEnum.GetAllDeployedEndpoints:
                    return GetAllDeployedEndpoints();
                default:
                    throw new LunaServerException($"Invalid operation type. The type is {nameof(operationType)}.");
            }
        }

        public async Task<bool> ExistsAsync(APIVersion version, Models.Azure.Operation operation)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat(), operation.name);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Get };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(operation), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return false;

            Models.Azure.Operation operationAPIM = (Models.Azure.Operation)System.Text.Json.JsonSerializer.Deserialize(responseContent, typeof(Models.Azure.Operation));
            if (operationAPIM == null)
            {
                throw new LunaServerException($"Query result in bad format. The response is {responseContent}.");
            }
            return true;
        }

        public async Task CreateAsync(APIVersion version, Models.Azure.Operation operation)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat(), operation.name);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(operation), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task UpdateAsync(APIVersion version, Models.Azure.Operation operation)
        {
            Uri requestUri = GetAPIVersionAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat(), operation.name);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Put };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(operation), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }

        public async Task DeleteAsync(APIVersion version, Models.Azure.Operation operation)
        {
            if (!(await ExistsAsync(version, operation))) return;

            Uri requestUri = GetAPIVersionAPIMRequestURI(version.ProductName, version.DeploymentName, version.GetVersionIdFormat(), operation.name);
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = HttpMethod.Delete };

            request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", _apimAuthHelper.GetSharedAccessToken());
            request.Headers.Add("If-Match", "*");

            request.Content = new StringContent(JsonConvert.SerializeObject(operation), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LunaServerException($"Query failed with response {responseContent}");
            }
        }
    }
}
