# Module 8: Diagnostics and Monitoring

1. Wherever you see a path to file starting at **[Repository Root]**, replace it with the absolute path to the directory in which the 20487 repository resides.
   e.g. - you cloned or extracted the 20487 repository to **C:\Users\John Doe\Downloads\20487**, then the following path: *[Repository Root]***\AllFiles\20487D\Mod01** will become **C:\Users\John Doe\Downloads\20487\AllFiles\20487D\Mod01**.
2. Wherever you see **{YourInitials}**, replace it with your actual initials. For example, the initials for **John Doe** will be **jd**).
3. Before performing the demonstration, you should allow some time for the provisioning of the different Microsoft Azure resources required for the demonstration. It is recommended to review the demonstrations before the actual class and identify the resources and then prepare them beforehand to save classroom time.

# Lab: Monitoring ASP.NET Core with ETW and LTTng

### Exercise 1: Collect and view ETW events

#### Task 1: Run the ASP.NET Core application

1. Open the command prompt.
2. To change directory to **Blueyonder.Service** project, run the following command:
   ```bash
   cd [Repository Root]\AllFiles\Mod08\Labfiles\Lab1\Blueyonder.Service
   ```
3. To run the service, run the following command:
   ```bash
   dotnet run
   ```

#### Task 2: Record .NET ETW events in PerfView

[Install PerfView](https://github.com/Microsoft/perfview/blob/master/documentation/Downloading.md)

1. Open PerfView.
2. In the top menu bar, click **Collect** and then click on **Collect** again.
3. Click **Start Collection**.

#### Task 3: Run a script to invoke service and make it to throw exceptions

1. Open Powershell.
2. To change directory to the **Assets** folder, run the following command:
   ```bash
   cd [Repository Root]\AllFiles\Mod08\Labfiles\Lab1\Assets
   ```
3.  To run the script that invokes the service 10 times, run the following command:
   ```bash
   .\requestsToServer
   ```
4. Close the command prompt.
4. Switch to **PerfView**.
5. Click **Stop Collection**, and then wait for the app to load the data.

#### Task 4: View exception details and call stacks in PerfView

1. Double-click **PerfViewData.etl.zip**.
2. Expand the **Advanced Group** folder, and then double-click **Exceptions Stacks**.
3. In the **Select Process window**, locate the following process:
   - In the **Name** column, find **dotnet**.
   - In the **CommandLine** column, find **dotnet exec** *[Repository Root]***\AllFiles\Mod08\Labfiles\Lab1\Blueyonder.Service**.

![perfview](./_images/perfview.png)

   - Double-click the process.
4. In the **Exception Stacks** window, click the **By Name** tab.
5. In the **Name** column, double-click **Throw(System.Exception) Id** *{number}* **can't be hash to flight code**.
6. View the exception call stack, find the line with the text **Blueyonder.Service.Controllers.FlightsControllers.Get(int32)**
7. This is the service that throw the exception.
8. On the top bar, in the **GroupPats** list, select **[no grouping]**.
9. Expand **Blueyonder.Service.Repository.FlightsRepository.HashFlightCode(int32)**.
10. View the exception call stack to see which method in the service throws the exception.
   > **Note**: The **HashFlightCode** method throws an exception and continues to **GetFlightCode**.

### Exercise 2: Collect and view LTTng events

#### Task 1: Run the ASP.NET Core application in a Linux container with COMPlus_EventLogEnabled=1

1. Open the command prompt.
2. To change directory to the **Blueyonder.Service** project, run the following command:
   ```bash
   cd [Repository Root]\AllFiles\Mod08\Labfiles\Lab1\Blueyonder.Service
   ```
3. To build the docker image, run the following command:
   ```bash
   docker build -t monitor .
   ```
4. To run the new container with the monitor image, run the following command:
   ```bash
   docker run -d -p 8080:80 --name myapp monitor
   ```

#### Task 2: Record LTTng events with the Lttng CLI tool

1. To enter the shell in the container, run the following command:
   ```
   docker exec -it myapp bash
   ```
2. To update packages in the container, run the following command:
   ```bash
   apt update
   ```
3. To install **software-properties-common**, run the following command:
   ```bash
   apt-get install software-properties-common
   ```
5. To update the list of packages, run the following command:
   ```bash
   apt-get update
   ```
6. To install the main **LTTng** packages, run the following command:
   ```bash
   apt-get install lttng-tools lttng-modules-dkms liblttng-ust-dev
   ```
7. To create a new **LTTng** session, run the following command:
   ```bash
   lttng create sample-trace
   ```
8. To add context data (process id, thread id, process name) to each event, run the following commands:
   ```bash
   lttng add-context --userspace --type vpid
   lttng add-context --userspace --type vtid
   lttng add-context --userspace --type procname
   ```
9. To create an event rule to record all the events starting with **DotNETRuntime**, run the following command:
   ```bash
   lttng enable-event --userspace --tracepoint DotNETRuntime:*
   ```
10. To start recording events, run the following command:
    ```bash
    lttng start
    ```
11. Open the browser and navigate to the following URL:
    ```url
    http://localhost:8080/api/flights/{id}
    ```
    > **Note**: Replace *{id}* with an int number.
12. Refresh the page a few times.


#### Task 3: View LTTng exception and GC events with the babeltrace CLI tool

1. To stop the recording, run the following command:
   ```bash
   lttng stop
   ```
2. To destroy the session, run the following command:
   ```bash
   lttng destroy
   ```
3. To see all the recorded events, paste the following command, press the Tab key, and then press the Enter key.
   ```bash
   babeltrace /root/lttng-traces/sample-trace
   ```
4. Explore all the events and look for the exception and GC events.

#### Task 4: Open the recording file on Windows with PerfView

1. To install **Zip** package, run the following command:
   ```bash
   apt-get install zip
   ```
2. To archive the trace folder, paste the following command, press the Tab key, and then press the Enter key:
   ```bash
   zip -r sample.trace.zip /root/lttng-traces/sample-trace
   ```
3. To exit bash, run the following command:
   ```bash
   exit
   ```
4. To copy the archive to the local file system, run the following command:
   ```bash
   docker cp myapp:app/sample.trace.zip .
   ```
5.  To kill the running container, run the following command:
   ```bash
   docker kill myapp
   ```
6. To remove the container, run the following command:
   ```bash
   docker rm myapp
   ```
7. Open the zip file in **PerfView** and look at the events.
8. Close all windows.


# Lab: Monitoring Azure Web Apps with Application Insights

### Preparation Steps

1. Open PowerShell as **Administrator**.
2. In the **User Account Control** modal, click **Yes**.
3. Run the following command: **Install-Module azurerm -AllowClobber -MinimumVersion 5.4.1**.
   >**Note**: If prompted for trust this repository, Type **A** and then press **Enter**.
4. Navigate to *[repository root]***\AllFiles\Mod08\Labfiles\Lab2\Setup**.
5. Run the following command:
    ```batch
     .\createAzureServices.ps1
    ```
6. You will be asked to supply a subscription ID, which you can get by performing the following steps:
    1. Open a browser and navigate to **http://portal.azure.com**. If a page appears, asking for your email address, enter your email address, and then click **Continue**. Wait for the **Sign-in** page to appear, enter your email address and password, and then click **Sign In**.
    2. On the top bar, in the search box, type **Cost**, and then in results, click **Cost Management + Billing(Preview)**. The **Cost Management + Billing** window opens.
    3. Under **BILLING ACCOUNT**, click **Subscriptions**.
    4. Under **My subscriptions**, you should have at least one subscription. Click on the subscription that you want to use.
    5. Copy the value from **Subscription ID**, and then paste it at the **PowerShell** prompt. 
7. In the **Sign in** window that appears, enter your details, and then sign in.
8. In the **Administrator: Windows PowerShell** window, follow the on-screen instructions. Wait for the deployment to complete successfully.
9.  Write down the name of the Azure App Service that is created.
10. Close the **PowerShell** window.


### Exercise 1: Add the Application Insights SDK

#### Task 1: Add the Application Insights SDK to the web service project

1. Open **Azure Portal**.
2. Click **All resources**, and then click **blueyondermod08lab2**{YourInitials}.
3. In the left menu, in the **SETTINGS** section, click **Application Insights**.
4. Click **Turn on site extension**, and then add the following information:
    - Select **Create new resource**.
    - Click **Apply** and in the **Apply monitoring settings** dialog box, click **Yes**.
5. In the **Application Insights** blade, click **View Application Insights data**.
6. Copy **Instrumentation Key**.
7. Open the command prompt.
8. To change directory to the **Starter** folder, run the following command:
    ```bash
    cd [Repository Root]\Allfiles\Mod08\Labfiles\Lab2\Starter
    ```
9.  Run the following command to install the **Microsoft.ApplicationInsights.AspNetCore** NuGet package:
    ```base
    dotnet add package Microsoft.ApplicationInsights.AspNetCore --version=2.4.1
    ```
10. To open the project in Microsoft Visual Studio Code, run the following command: 
    ```bash
    code .
    ```
11. Click **appsettings.json**, and then add the following code:
    ```json
    "ApplicationInsights": {
        "InstrumentationKey": "{InstrumentationKey}"
    }
    ```
12. Replace the **InstrumentationKey** key with value copied in step 6.
13. Locate the **CreateWebHostBuilder** lamda in the **Progam** class and replace it with the following code:
    ```cs
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseApplicationInsights();
    ```


#### Task 2: Publish the service to Azure Web Apps

1. Switch to the command prompt.
2. To publish the service, paste the following command:
    ```bash
    dotnet publish /p:PublishProfile=Azure /p:Configuration=Release
    ```

### Exercise 2: Load test the web service

#### Task 1: Create a new performance test in the Azure Portal

1. Switch to Azure Portal.
2. Click **All resources**, and then click the **blueyondermod08lab2**{YourInitials} resource from type of **Application Insights**.
3. On the right pane, Click **View Applications Insights data** in the **Configure** section, click **Performance testing**.
4. On the top menu bar, click **Set Organization**, and then add the following information:
    - In **Organization Settings(Preview)**, click **Or Create New**, and then type **blueyondervsts**{YourInitials}.
    - In **Subscription**, click **Azure Pass**.
    - Click **OK**.
5. Click **All resources**, and then click **blueyondervsts**.
6. To navigate to Visual Studio Team Services (VSTS), click the **Url** value.
7. In VSTS, sign in with your credentials.
8. To create a new project in VSTS, in **Create new project**, add the following information:
    - In **Project name**, type **Blueyonder**.
    - Click **Create Project**.

#### Task 2: Run the performance test for a few minutes with multiple simulated users

1. Switch to Azure Portal.
2. Click **All resources**, and then click the **blueyondermod08lab2**{YourInitials} resource form type of **Application Insights**.
3. On the left menu, in the **Configure** section, click **Performance Testing**.
4. On the top menu bar, click **New**, and then add the following information:
    - Click **CONFIGURE TEST USING**:
        - In **TEST TYPE**, select **Manual Test**.
        - In **URL**, type **http://blueyondermod08lab2{YourInitials}.azurewebsites.net/api/destinations**.
        - Click **Done**.
    - In **Name**, type **DestinationsTest**.
    - In **USER LOAD**, type **20**.
    - In **DURATION (MINUTES)**, type **5**.
    - Click **Run test**.

### Exercise 3: Analyze the performance results

#### Task 1: View the overall website performance and request metrics

1. Switch to VSTS.
2. On the left pane, place the mouse pointer over **Test Plans**, and then click **Load test**.
3. Double-click **DestinationsTest**.
4. View the summary of the test.
5. To view the the overall website performance of the test, click the **Charts** tab.

#### Task 2: Examine specific requests and view their timelines and dependencies

1. Switch to the **azure portal**, in the investigate section click **performance**.
2. Scroll down and locate **OPERATION NAME**.
3. Click **GET destinations/Get**.
4. On the right-hand side, view the info about the request.

#### Task 3: Drill down into the Application Insights Profiler results for slow requests

1. In the right-hand bottom corner, click **Samples**.
2. View all the requests, and then click the request with the highest duration.
3. View all the details about the request.

©2018 Microsoft Corporation. All rights reserved.

The text in this document is available under the [Creative Commons Attribution 3.0 License](https://creativecommons.org/licenses/by/3.0/legalcode), additional terms may apply. All other content contained in this document (including, without limitation, trademarks, logos, images, etc.) are **not** included within the Creative Commons license grant. This document does not provide you with any legal rights to any intellectual property in any Microsoft product. You may copy and use this document for your internal, reference purposes.

This document is provided &quot;as-is.&quot; Information and views expressed in this document, including URL and other Internet Web site references, may change without notice. You bear the risk of using it. Some examples are for illustration only and are fictitious. No real association is intended or inferred. Microsoft makes no warranties, express or implied, with respect to the information provided here.
