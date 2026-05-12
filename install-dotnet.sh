#!/bin/bash
set -e

echo "正在下载 .NET 8.0 SDK..."

wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh

echo "正在安装 .NET 8.0 SDK..."
./dotnet-install.sh --channel 8.0 --install-dir /opt/dotnet

export DOTNET_ROOT=/opt/dotnet
export PATH=$PATH:/opt/dotnet

echo 'export DOTNET_ROOT=/opt/dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:/opt/dotnet' >> ~/.bashrc

echo "正在验证安装..."
/opt/dotnet/dotnet --version

echo "✅ .NET 8.0 SDK 安装完成！"
echo "运行 'source ~/.bashrc' 或重新打开终端使 PATH 生效"
