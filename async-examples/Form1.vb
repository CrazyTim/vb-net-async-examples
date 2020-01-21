Public Class Form1

    ' refer: https://docs.microsoft.com/en-us/dotnet/visual-basic/programming-guide/concepts/async/

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Await Test_ShowAsyncDialog(Me)
        Await Test_AsyncCalls()
        Me.Close()

    End Sub

    Public Async Function Test_AsyncCalls() As Task

        ' nb: always wrap in try if in a sub, as it will not be caught and crash app

        Dim s As String = ""

        Try
            s = Await SlowFunctionAsyncWrapper("1", 2000)
        Catch ex As Exception
            Console.WriteLine($"1: exception: {ex.Message}")
        End Try

        Application.DoEvents()

        Try
            s = Await SlowFunctionAsyncWrapper("2", 2500).TimeoutAfter(2000) ' this will timeout!!
        Catch ex As Exception ' nb: catch timeout here
            Console.WriteLine($"2: exception: {ex.Message}")
        End Try

        Application.DoEvents() ' nb: need do events here because textbox won't have enough time to update before the GUI thread is frozen when we call AccessTheWeb

        Try
            s = SlowFunctionAsyncWrapper("3", 2000).Result ' this task will execute syncronously and block the GUI thread
        Catch ex As Exception
            Console.WriteLine($"3: exception: {ex.Message}")
        End Try

        Application.DoEvents()

        Try
            s = Await SlowFunctionAsyncWrapper("4", 1500)
        Catch ex As Exception
            Console.WriteLine($"4: exception: {ex.Message}")
        End Try

        Application.DoEvents()


        ' workaround if you want to await a sub (but really you should always use a function):
        Try
            Await Task.Run(Sub() SlowSub("5", 1500)) ' call the slow task
        Catch ex As Exception
            Console.WriteLine($"sub: exception: {ex.Message}")
        End Try

    End Function

    Public Async Function Test_ShowAsyncDialog(ByVal ParentForm As Form) As Task

        Using f As New Form() With {
                .Text = "Loading...",
                .Width = 300,
                .Height = 100,
                .StartPosition = FormStartPosition.CenterParent
            }

            ParentForm.BeginInvoke(Sub() f.ShowDialog(ParentForm)) ' show non-blocking dialog

            Await SlowAsyncFunction("1", 2000)

            f.Close()
        End Using

    End Function

End Class
