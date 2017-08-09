Function GetProductVersion (sFilePath, sProgram)
Dim objShell, objFolder, objFolderItem, i
Set fso = CreateObject("Scripting.FileSystemObject")
If fso.FileExists(sFilePath & "\" & sProgram) Then
    Set objShell = CreateObject("Shell.Application")
    Set objFolder = objShell.Namespace(sFilePath)
    Set objFolderItem = objFolder.ParseName(sProgram)
    Dim arrHeaders(400)
    For i = 0 To 400
        arrHeaders(i) = objFolder.GetDetailsOf(objFolder.Items, i)
        'WScript.Echo i &"- " & arrHeaders(i) & ": " & objFolder.GetDetailsOf(objFolderItem, i)
        If lcase(arrHeaders(i))= "product version" Then
            GetProductVersion= objFolder.GetDetailsOf(objFolderItem, i)
            Exit For
        End If
        If lcase(arrHeaders(i))= "versão do produto" Then
            GetProductVersion= objFolder.GetDetailsOf(objFolderItem, i)
            Exit For
        End If
    Next
End If
End Function

set args = WScript.Arguments
WScript.Echo GetProductVersion(args(0), args(1))
WScript.Quit