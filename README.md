# A Step By Step TM3 Client File Store (CFS) File API using C#
- version: 1.0.0
- Last update: August 2025
- Environment: C# and .NET 8.0

Example Code Disclaimer:
ALL EXAMPLE CODE IS PROVIDED ON AN “AS IS” AND “AS AVAILABLE” BASIS FOR ILLUSTRATIVE PURPOSES ONLY. LSEG MAKES NO REPRESENTATIONS OR WARRANTIES OF ANY KIND, EXPRESS OR IMPLIED, AS TO THE OPERATION OF THE EXAMPLE CODE, OR THE INFORMATION, CONTENT, OR MATERIALS USED IN CONNECTION WITH THE EXAMPLE CODE. YOU EXPRESSLY AGREE THAT YOUR USE OF THE EXAMPLE CODE IS AT YOUR SOLE RISK.

## <a id="prerequisite"></a> Prerequisite

This example requires the following dependencies.

1. Visual Studio 2022 IDE.
2. [.NET SDK version8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
3. RDP access credential with your TM3 Package ID.
4. Internet connection.

Please contact your LSEG representative to help you to access the RDP account and TM3 access.

## <a id="how_to_run"></a>How to run the demo application

The first step is to unzip or download the example project folder into a directory of your choice, then set up the environment by the steps below.

### <a id="csharp_example_run"></a>Run the demo Console application with Visual Studio 2022 IDE

1. Create a file name **.env** inside the **TM3Console** folder with the following content

    ```ini
    MACHINE_ID=<RDP Username>
    PASSWORD=<RDP Password>
    APP_KEY=<RDP App Key>
    PACKAGE_ID=<Your Bulk Package ID>
    ```
2. Open the **TM3Console.sln** solution file on Visual Studio 2022 IDE.
3. Run the project using Visual Studio IDE.
4. The TM3 bulk file will be downloaded to **TM3Console\bin\Debug\net8.0** folder.

### <a id="csharp_example_run"></a>Run the demo Console application with .NET CLI

1. Open the Command Prompt and go to the project's **TM3Console** folder.
2. Create a file name **.env** inside the **TM3Console** folder with the following content

    ```ini
    MACHINE_ID=<RDP Username>
    PASSWORD=<RDP Password>
    APP_KEY=<RDP App Key>
    PACKAGE_ID=<Your Bulk Package ID>
    ```
3. Open the **TM3Console** folder via a command prompt application
4. Run the following command to build a solution

    ```bash
    $>dotnet build
    ```
5. Once the building process is completed, run the following command to start an application

    ```bash
    $>dotnet run
    ```
6. The TM3 bulk file will be downloaded to **TM3Console\bin\Debug\net8.0** folder.
