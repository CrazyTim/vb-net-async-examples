Imports System.Runtime.CompilerServices

Module Module1

    ''' <summary> Set a timeout for the task. A TimeoutException will be raised. </summary>
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

    ' if awaited, this function will not block the gui.
    Public Async Function SlowAsyncFunction(ByVal p As String, ByVal Delay As Integer) As Task

        Dim sw As Stopwatch
        sw = Stopwatch.StartNew()

        Await Task.Delay(Delay)

        Console.WriteLine($"{p}: {sw.ElapsedMilliseconds}ms")

    End Function

    ' this function that will block the gui
    Public Function SlowFunction(ByVal p As String, ByVal Delay As Integer) As String

        Dim sw As Stopwatch
        sw = Stopwatch.StartNew()

        Threading.Thread.Sleep(Delay)

        Console.WriteLine($"{p}: {sw.ElapsedMilliseconds}ms")

        Return p

    End Function

    ' this sub that will block the gui
    Public Sub SlowSub(ByVal p As String, ByVal Delay As Integer)

        Dim sw As Stopwatch
        sw = Stopwatch.StartNew()

        Threading.Thread.Sleep(Delay)

        Console.WriteLine($"{p}: {sw.ElapsedMilliseconds}ms")

    End Sub

    ' this is how you build an async wrapper for a slow task
    Public Function SlowFunctionAsyncWrapper(ByVal p As String, ByVal Delay As Integer) As Task(Of String)

        Dim t As Task(Of String) = Task.Run(Function() SlowFunction(p, Delay)) ' call the slow task

        Return t

    End Function

End Module
