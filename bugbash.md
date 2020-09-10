<!-- markdownlint-disable MD033 -->
# IoT Plug and Play Samples Bug Bash instructions for the GA Sept 30th 2020 release

## Intro

IoT Plug and Play will be GA the Sept 30th 2020.

This bug bash is focused on the  IoT Hub features, SDKs, and tooling. We have quickstarts in various languages ready, for device and service scenarios.

### BugBash support and feedback

- Use the teams channel [PnP GA BugBash](https://teams.microsoft.com/l/meetup-join/19%3ameeting_M2I2NDc5OGItZmU4MC00MWU4LWE1MTQtNDI2YWE0MTlmZmYx%40thread.v2/0?context=%7b%22Tid%22%3a%2272f988bf-86f1-41af-91ab-2d7cd011db47%22%2c%22Oid%22%3a%22a43f0cf6-a7bc-4985-aa0b-37503f8ea92a%22%7d) to ask for help. There will be team members answering questions during the BugBash.

- If you have bugs or feature request, please use this [Bug Template](https://msazure.visualstudio.com/One/_workitems/create/Bug?templateId=221e542f-3428-49ba-951b-5ba1dce3f9a9&ownerId=1e65a829-00c0-4dc9-8088-d41678a0d033). You can query existing bugs in this [PnP BugBash Query](https://msazure.visualstudio.com/One/_queries/query-edit/07523176-81f2-4eb3-a795-8a483cd30310/)

### Getting started, environment and tooling

#### Docs

- The pnp docs are available URL: [IoT Plug and Play documentation](https://docs.microsoft.com/en-us/azure/iot-pnp/). To provide feedback use this [PR in docs](https://github.com/MicrosoftDocs/azure-docs-pr/pull/120981), your github account must be registered in the MicrosoftDocs org.



#### IoT Hub

- The required hub version is only available in the following regions: canary (EastUS2EUAP, CentralUSEUAP) and production (ESD 1-4). For Canary either use a Canary enabled subscription to create an IoT Hub or request access to the `IOTPNP_TEST_BY_MAIN` subscription, via the Teams channel mentioned above.
- Create S1 IoTHub in any of these regions to get started.
- Use the latest API version: 2020-05-31-preview

>NOTE: All hubs created in the `IOTPNP_TEST_BY_MAIN` subscription will be removed after the bug bash. We recommend using your own subscription if you'd want to keep using your IoT Hub long term.

To create the hub using the `az` CLI replace the hubname and run the script below:

```bash
az extension add --name azure-iot
az login
az account set -s IOTPNP_TEST_BY_MAIN
az iot hub create --resource-group BugBash --sku S1 --location eastus2euap --partition-count 4 --name <alias-hub-name>
```

To create the hub from the portal make sure you select the right subscription, region and resource group.

> Note. Make sure you get the IoT Hub connection string to be able to configure IoT explorer.

#### Digital Twin Definition Language

The DTDL v2 Spec can be found at [https://aka.ms/dtdl](https://aka.ms/dtdl) can be used as a reference for the language. Use the [samples](https://github.com/Azure/opendigitaltwins-dtdl/tree/master/DTDL/v2/samples) we've provided to get started.

- [DTDL VS Code extension](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.vscode-dtdl)
- [DTDL VS 2019 extension](https://github.com/rido-min/pnp-summer-bugbash/releases/tag/1)

#### PnP related tools

Tools available as internal previews are:

There is no Model repository for GA, this will land later.

- [Azure IoT Explorer](https://github.com/YingXue/azure-iot-explorer/releases/tag/PnpSummerRefresh-0709) (use bugbash.Azure.IoT.Explorer.preview.0.11.1.msi
)

### Option 1. Review and run SDK samples

All samples implement the Themorstat and TemperatureController models available in the [DTDL spec repo](https://github.com/Azure/opendigitaltwins-dtdl/tree/master/DTDL/v2/samples), and use the same validation flow:

1. Prepare source code for each language.
2. Create a device and provide the connection string.
3. Configure IoT Explorer to find the models and to interact with the device.

Availabe quickstarts and tutorials:
1. You can test the 5 quickstart with Device SDK  (C, C#, Java, Node, Python)
1. You can test the 2 quickstart with Service SDK (Node, Python)
1. You can also test the 5 Tutorial with multi component devices (C#, Java, Node, Python)

All quickstarts and tutorials are in the [IoT Plug and Play documentation](https://docs.microsoft.com/en-us/azure/iot-pnp/)

You can also test the **Quickstart on Azure RTOS for STM32L475 device**, [download this zip file](https://microsoft-my.sharepoint.com/:u:/p/liydu/ETyQje64iM1Iu_xpWpG8Kp0BxHZJK-QtFPrqjOl_PJdBNA?e=NGGqRl) and follow the instructions in the readme pdf file. 

### Option 2. Exploratory testing

Create simulated device and solution from scratch

- Follow the instructions in this [deck](https://microsoft.sharepoint.com/:p:/t/PnPCross-TeamCore/Ed9pGHB_AaxIgisKioEHSygB2mADMo2vPSQJZK0lKBnFKQ?e=VZ1ztQ) to create your own simulated device and custom solution from scratch.
- The instructions in that deck are specific to C# but can be generalized to the language of your choice as needed.