# ESD-Download-Links-Viewer

This tool is for Windows 10 Insiders easily to get the new Insider Preview Build's ESD file download link and then share for others.

For this purpose, you must finished your system's upgrade before you run this tool.

Because it reads the C:\Windows.old\WINDOWS\SoftwareDistribution\DataStore\DataStore.edb file.

To read this file properly, the program has to run with Administrative privilege.

You can also open a DataStore.edb file manually.

This tool's UI is based on WPF, fully vectorize and support Per-Monitor DPI scale. But you have to install .Net Framework 4.6.2 to support Per-Monitor DPI scaling.


## Chinese Description 中文简介

这个工具专门用于参与Windows 10 Insider测试的会员在更新新版Insider预览版后能够轻松分享新版Insider预览版的esd安装文件的下载链接。

下载链接具有时效性，本工具会在界面上告知用户链接的有效时间（软件界面上会转换为本地时间显示），一键复制出来的详细分享信息也会包含这个时间信息（但会以UTC时间显示）。

为了让这个工具能够正确运行，你必须在自己完成新版系统更新后才能运行使用，并且请确保你更新完后还没清理旧的Windows安装，因为本程序需要读取 C:\Windows.old\WINDOWS\SoftwareDistribution\DataStore\DataStore.edb 文件。

另外，为了让程序能成功读取到这个路径的数据库文件，程序强制使用管理员权限运行。

你也可以手动打开一个你存放到其他路径的DataStore.edb文件，软件界面能筛选显示数据库里的所有文件下载记录或者筛选只显示ESD文件的记录。每一个记录都能查看对应的文件SHA-1以及SHA-256的Hash数值。

本程序的界面基于WPF技术开发，完全矢量化，并且支持Per-Monitor DPI缩放（独立显示器DPI缩放，或叫动态DPI缩放），但为了支持Per-Monitor DPI缩放，你必须安装.Net Framework 4.6.2。程序最低要求.Net Framework 4.5（但不支持Per-Monitor DPI缩放）。
