#!/bin/bash

# QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
# Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

full_path=$(realpath $0)
current_dir=$(dirname $full_path)
default_image=quantconnect/lean:latest
default_data_dir=$current_dir/Data
default_results_dir=$current_dir
default_config_file=$current_dir/Launcher/config.json
default_python_dir=$current_dir/Algorithm.Python/
csharp_dll=$current_dir/Launcher/bin/Debug/QuantConnect.Algorithm.CSharp.dll
csharp_pdb=$current_dir/Launcher/bin/Debug/QuantConnect.Algorithm.CSharp.pdb

#If arg is a file process the key values
if [ -f "$1" ]; then
    IFS="="
    while read -r key value; do
        eval "$key='$value'"
    done < $1
#If there are in line args, process them
elif [ ! -z "$*" ]; then
    for arg in "$@"; do
        eval "$arg"
    done
#Else query user for settings
else
    read -p "Enter docker image [default: $default_image]: " image
    read -p "Enter absolute path to Lean config file [default: $default_config_file]: " config_file
    read -p "Enter absolute path to Data folder [default: $default_data_dir]: " data_dir
    read -p "Enter absolute path to store results [default: $default_results_dir]: " results_dir
    read -p "Would you like to debug C#? (Requires mono debugger attachment) [default: N]: " debugging
fi

if [ -z "$image" ]; then
    image=$default_image
fi

if [ -z "$config_file" ]; then
    config_file=$default_config_file
fi

if [ -z "$python_dir" ]; then
    python_dir=$default_python_dir
fi

if [ ! -f "$config_file" ]; then
    echo "Lean config file $config_file does not exist"
    exit 1
fi

if [ -z "$data_dir" ]; then
    data_dir=$default_data_dir
fi

if [ ! -d "$data_dir" ]; then
    echo "Data directory $data_dir does not exist"
    exit 1
fi

if [ -z "$results_dir" ]; then
    results_dir=$default_results_dir
fi

if [ ! -d "$results_dir" ]; then
    echo "Results directory $results_dir does not exist"
    exit 1
fi

#First part of the docker command that is static, then we build the rest
command="docker run --rm --mount type=bind,source=$config_file,target=/Lean/Launcher/config.json,readonly \
    --mount type=bind,source=$data_dir,target=/Data,readonly \
    --mount type=bind,source=$results_dir,target=/Results \
    --name LeanEngine \
    -p 5678:5678 "

#If the csharp dll and pdb are present, mount them
if [ ! -f "$csharp_dll" ]; then
    echo "Csharp file at '$csharp_dll' does not exist; no CSharp files will be mounted"
else 
    command+="--mount type=bind,source=$csharp_dll,target=/Lean/Launcher/bin/Debug/QuantConnect.Algorithm.CSharp.dll \
    --mount type=bind,source=$csharp_pdb,target=/Lean/Launcher/bin/Debug/QuantConnect.Algorithm.CSharp.pdb "
fi

#If python algorithms are present, mount them
if [ ! -d "$python_dir" ]; then
    echo "No Python Algorithm location found at '$python_dir'; no Python files will be mounted"
else
    command+="--mount type=bind,source=$python_dir,target=/Lean/Algorithm.Python "
fi

#If debugging is set then set the entrypoint to run mono with a debugger server
shopt -s nocasematch
if [[ "$debugging" == "y" ]]; then
    command+="-p 55555:55555 \
    --entrypoint mono \
    $image --debug --debugger-agent=transport=dt_socket,server=y,address=0.0.0.0:55555,suspend=y \
    QuantConnect.Lean.Launcher.exe --data-folder /Data --results-destination-folder /Results --config /Lean/Launcher/config.json"

    echo "Docker container starting, attach to Mono process at localhost:55555 to begin"
else 
    command+="$image --data-folder /Data --results-destination-folder /Results --config /Lean/Launcher/config.json"
fi

#Run built docker command; docker requires sudo privledges by default
eval sudo $command
