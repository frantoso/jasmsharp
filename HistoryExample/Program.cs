// -----------------------------------------------------------------------
// <copyright file="Program.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

using HistoryExample;
using jasmsharp_debug_adapter;


var controller = new Controller();

var adapter = new DebugAdapter(controller.MainFsm.Machine);

controller.Run();