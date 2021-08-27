<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/253582825/20.1.1%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T878651)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
# How to Use InfiniteAsyncSource In Single Thread

This example demonstrates how to bind the [GridControl](http://docs.devexpress.com/WPF/DevExpress.Xpf.Grid.GridControl?v=20.1) to [InfiniteAsyncSource](http://docs.devexpress.com/WPF/DevExpress.Xpf.Data.InfiniteAsyncSource?v=20.1) that works in a single thread.

To make [InfiniteAsyncSource](http://docs.devexpress.com/WPF/DevExpress.Xpf.Data.InfiniteAsyncSource?v=20.1) work in a single thread, use a custom task scheduler. See the **SingleThreadTaskScheduler** class in the example.
