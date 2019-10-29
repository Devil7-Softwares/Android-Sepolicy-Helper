Imports System.Text.RegularExpressions

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs)
        ListBox1.Items.Clear()
        Dim R As New Regex(".*: avc: denied \{ (?<action>.*) \} for .* scontext=u:r:(?<source>.*):s0 tcontext=u:object_r:(?<target>.*):s0 tclass=(?<class>.*) permissive=.*")
        Dim Groups As GroupCollection = R.Match(TextBox1.Text).Groups
        For Each i As Group In Groups
            ListBox1.Items.Add(i.Value)
        Next
    End Sub
End Class
