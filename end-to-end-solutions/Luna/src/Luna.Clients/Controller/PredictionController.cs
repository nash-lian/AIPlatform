﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Luna.Clients.Controller
{
    public class PredictionController : IController
    {
        public PredictionController()
        { 
        }

        public string GetName()
        {
            return "predict";
        }

        public string GetUrlTemplate()
        {
            return "/predict";
        }

        public string GetMethod()
        {
            return "POST";
        }

        public string GetPath(string productName, string deploymentName)
        {
            return $"/api/products/{productName}/deployments/{deploymentName}";
        }

        public string GetBaseUrl()
        {
            return "https://lunaaitest-apiapp.azurewebsites.net";
        }
    }
}
