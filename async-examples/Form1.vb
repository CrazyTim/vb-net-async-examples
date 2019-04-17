Imports System.Net.Http
Imports System.Runtime.CompilerServices

Public Class Form1


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        TestAsync()

    End Sub


    ' 1) this is an example of a function that will block the GUI thread
    Private Function SlowTaskFunction(ByVal p As String, ByVal Delay As Integer) As String

        Dim sw As Stopwatch
        sw = Stopwatch.StartNew()

        Threading.Thread.Sleep(Delay)

        Console.WriteLine($"SlowTaskFunction({p}): {sw.ElapsedMilliseconds}ms")

        Return p

    End Function


    Private Sub SlowTaskSub(ByVal Delay As Integer)

        Dim sw As Stopwatch
        sw = Stopwatch.StartNew()

        Threading.Thread.Sleep(Delay)

        Console.WriteLine($"SlowTaskSub: {sw.ElapsedMilliseconds}ms")

    End Sub


    ' 2) this is how you build an aysnc wrapper for it
    Private Function SlowTaskAsync(ByVal p As String, ByVal Delay As Integer) As Task(Of String)

        Dim t As Task(Of String) = Task.Run(Function() SlowTaskFunction(p, Delay)) ' call the slow task

        Return t

    End Function


    ' 3) testing async
    Public Async Sub TestAsync()

        ' nb: always wrap in try if in a sub, as it will not be caught and crash app

        Dim s As String = ""

        Try
            s = Await SlowTaskAsync("1", 2000)
        Catch ex As Exception
            Console.WriteLine($"1: exception: {ex.Message}")
        End Try

        Application.DoEvents()

        Try
            s = Await SlowTaskAsync("2", 2500).TimeoutAfter(2000) ' this will timeout!!
        Catch ex As Exception ' nb: catch timeout here
            Console.WriteLine($"2: exception: {ex.Message}")
        End Try

        Application.DoEvents() ' nb: need do events here because textbox won't have enough time to update before the GUI thread is frozen when we call AccessTheWeb

        Try
            s = SlowTaskAsync("3", 2000).Result ' this task will execute syncronously and block the GUI thread
        Catch ex As Exception
            Console.WriteLine($"3: exception: {ex.Message}")
        End Try

        Application.DoEvents()

        Try
            s = Await SlowTaskAsync("4", 1500)
        Catch ex As Exception
            Console.WriteLine($"4: exception: {ex.Message}")
        End Try

        Application.DoEvents()


        ' await a slow sub
        ' no way to timeout sub atm :(
        Try
            Await Task.Run(Sub() SlowTaskSub(1500)) ' call the slow task
        Catch ex As Exception
            Console.WriteLine($"sub: exception: {ex.Message}")
        End Try

    End Sub


End Class


Public Module test


    ' this very useful extension allows you to put a timeout on a task
    <Extension()>
    Public Async Function TimeoutAfter(Of TResult)(ByVal task As Task(Of TResult), ByVal TimeoutMilliseconds As Long) As Task(Of TResult)

        ' adapted from: https://stackoverflow.com/a/22078975/737393

        Using timeoutCancellationTokenSource = New Threading.CancellationTokenSource()

            Dim completedTask

            If TimeoutMilliseconds = 0 Then
                completedTask = Await task.WhenAny(task)
            Else
                completedTask = Await task.WhenAny(task, task.Delay(New TimeSpan(TimeoutMilliseconds * TimeSpan.TicksPerMillisecond), timeoutCancellationTokenSource.Token))
            End If

            If completedTask Is task Then
                timeoutCancellationTokenSource.Cancel()
                Return Await task
            Else
                Throw New TimeoutException()
            End If

        End Using
    End Function


End Module
