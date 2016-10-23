Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("SPC.HMAC.Server")>
<Assembly: AssemblyDescription("Web API HMAC authentication (server component)")>
<Assembly: AssemblyCompany("SPC Internet Ltd")>
<Assembly: AssemblyProduct("SPC.HMAC.Server")>
'<Assembly: AssemblyCopyright("Copyright ©  2016")> 
<Assembly: AssemblyTrademark("")> 

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("3461b732-3e4e-47c3-bc09-155ca773754e")>

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("1.0.*")> 

<Assembly: AssemblyVersion("1.0.0.0")>
<Assembly: AssemblyFileVersion("1.0.2.0")>


#If DEBUG Then
<Assembly: AssemblyCopyright("Compiled as DEBUG")>
<Assembly: AssemblyMetadata("CompileMode", "Compiled as DEBUG")>
#Else
<Assembly: AssemblyCopyright("Compiled as RELEASE")>
<Assembly: AssemblyMetadata("CompileMode", "Compiled as RELEASE")>
#End If
