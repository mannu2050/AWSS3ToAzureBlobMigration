﻿using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ContentMigrationService
{
    public partial class MigrationService : ServiceBase
    {
        public MigrationService()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
           
        }

        protected override void OnStop()
        {
        }
    }
}
