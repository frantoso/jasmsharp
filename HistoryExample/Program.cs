// -----------------------------------------------------------------------
// <copyright file="Program.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

using System.Text.Json;
using HistoryExample;
using jasmsharp_debug_adapter;


var controller = new Controller();

var abc = new DebugAdapter(controller.MainFsm.Machine);
var json = JsonSerializer.Serialize(abc.FsmInfo);

controller.Run();