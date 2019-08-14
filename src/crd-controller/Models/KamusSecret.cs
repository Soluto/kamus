﻿using System.Collections.Generic;
using k8s;
using k8s.Models;

namespace CustomResourceDescriptorController.Models
{
    public class KamusSecret : KubernetesObject
    {
        public Dictionary<string, string> Data { get; set; }
        public Dictionary<string, string> EncodedData { get; set; }
        public string Type { get; set; }
        public V1ObjectMeta Metadata { get; set; }
        public string ServiceAccount { get; set; }
        
        public string Status { get; set; }
    }
}
