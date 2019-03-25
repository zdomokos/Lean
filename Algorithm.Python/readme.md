QuantConnect Python Algorithm Project:
=============

Before we enable python support, follow the [installation instructions](https://github.com/QuantConnect/Lean#installation-instructions) to get LEAN running C# algorithms in your machine. 

### Install Python 3.6:
#### [Windows](https://github.com/QuantConnect/Lean#windows)
1. Use the Windows x86-64 MSI installer from [python.org](https://www.python.org/downloads/release/python-366/) or [Anaconda](https://repo.anaconda.com/archive/Anaconda3-5.2.0-Windows-x86_64.exe) for Windows installer
2. When asked to select the features to be installed, make sure you select "Add python.exe to Path"
3. `[Optional]` Create `PYTHONHOME` system variables which value must be the location of your python installation (e.g. `C:\Python36amd64` or `C:\Anaconda3`):
   1. Right mouse button on My Computer. Click Properties.
   2. Click Advanced System Settings -> Environment Variables -> System Variables
   3. Click **New**. 
        - Name of the variable: `PYTHONHOME`. 
        - Value of the variable: python installation path.
4. Install [pandas](https://pandas.pydata.org/) and its [dependencies](https://pandas.pydata.org/pandas-docs/stable/install.html#dependencies).

#### [macOS](https://github.com/QuantConnect/Lean#macos)
1. Use the macOS x86-64 package installer from [Anaconda](https://repo.anaconda.com/archive/Anaconda3-5.2.0-MacOSX-x86_64.pkg) and follow "[Installing on macOS](https://docs.anaconda.com/anaconda/install/mac-os)" instructions from Anaconda documentation page.
2. Install [pandas](https://pandas.pydata.org/) and its [dependencies](https://pandas.pydata.org/pandas-docs/stable/install.html#dependencies).

#### [Linux](https://github.com/QuantConnect/Lean#linux-debian-ubuntu)
By default, **miniconda** is installed in the users home directory (`$HOME`):
```
export PATH="$HOME/miniconda3/bin:$PATH"
wget https://cdn.quantconnect.com/miniconda/Miniconda3-4.3.31-Linux-x86_64.sh
bash Miniconda3-4.3.31-Linux-x86_64.sh -b
rm -rf Miniconda3-4.3.31-Linux-x86_64.sh
sudo ln -s $HOME/miniconda3/lib/libpython3.6m.so /usr/lib/libpython3.6m.so
conda update -y python conda pip
conda install -y cython pandas
```

*Note:* There is a [known issue](https://github.com/pythonnet/pythonnet/issues/609) with python 3.6.5 that prevents pythonnet installation, please upgrade python to version 3.6.6:
```
conda install -y python=3.6.6
```

### Run python algorithm
1. Update the [config](https://github.com/QuantConnect/Lean/blob/master/Launcher/config.json) to run the python algorithm:
```json
"algorithm-type-name": "BasicTemplateAlgorithm",
"algorithm-language": "Python",
"algorithm-location": "../../../Algorithm.Python/BasicTemplateAlgorithm.py",
```
 2. Rebuild LEAN.
 3. Run LEAN. You should see the same result of the C# algorithm you tested earlier.

___
#### Python.Runtime.dll compilation
LEAN users do **not** need to compile `Python.Runtime.dll`. The information below is targeted to developers who wish to improve it. 

Download [QuantConnect/pythonnet](https://github.com/QuantConnect/pythonnet/) github clone or downloading the zip. If downloading the zip - unzip to a local pathway.

**Note:** QuantConnect's version of pythonnet is an enhanced version of [pythonnet](https://github.com/pythonnet/pythonnet) with added support for `System.Decimal` and `System.DateTime`.

Below we can find the compilation flags that create a suitable `Python.Runtime.dll` for each operating system.

**Windows**
```
msbuild pythonnet.sln /nologo /v:quiet /t:Clean;Rebuild /p:Platform=x64 /p:PythonInteropFile="interop36.cs" /p:Configuration=ReleaseWin /p:DefineConstants="PYTHON36,PYTHON3,UCS2"
```
**macOS**
```
msbuild pythonnet.sln /nologo /v:quiet /t:Clean;Rebuild /p:Platform=x64 /p:PythonInteropFile="interop36m.cs" /p:Configuration=ReleaseMono /p:DefineConstants="PYTHON36,PYTHON3,UCS4,MONO_OSX,PYTHON_WITH_PYMALLOC"
```
**Linux**
```
msbuild pythonnet.sln /nologo /v:quiet /t:Clean;Rebuild /p:Platform=x64 /p:PythonInteropFile="interop36m.cs" /p:Configuration=ReleaseMono /p:DefineConstants="PYTHON36,PYTHON3,UCS4,MONO_LINUX,PYTHON_WITH_PYMALLOC"
```
