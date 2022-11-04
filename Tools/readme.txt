一、环境
    python3.8,pyqt5
    requirements.txt中包含所有依赖

二、目录
    ui中是qtdesigner使用的文件 可以通过pyuic5直接导出代码
    eg.
        pyuic5 -x -o addui.py add.ui
    文件对应
        add.ui->addui.py
        main.ui->form.py
        setting.ui->settingui.py

三、生成exe
    使用pyinstaller export_tool.exe.spec