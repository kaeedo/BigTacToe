namespace BigTacToe.Wpf

open System

open Xamarin.Forms
open Xamarin.Forms.Platform.WPF

type MainWindow() = 
    inherit FormsApplicationPage()

module Main = 
    [<EntryPoint>]
    [<STAThread>]
    let main(_args) =

        let app = new System.Windows.Application()
        Forms.Init()
        let window = MainWindow()
        window.Height <- 1280.0
        window.Width <- 720.0
        
        window.LoadApplication(BigTacToe.App())

        app.Run(window)
