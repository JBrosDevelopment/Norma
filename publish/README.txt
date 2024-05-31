Thank you for trying Norma. The EXE is huge becasue it has been published with all of DotNet runtime inside it. This ensures it will run on computers that don't have dotnet sdk installed. 

Command I used to release it:

dotnet publish -o "publish" Norma.csproj -r win-x64 -c Release -p:PublishReadyToRun=true -p:PublishSingleFile=true --self-contained
         ^          ^            ^           ^          ^                 ^                         ^                     ^
publish executable  |      Project file      | Set to release mode        |                  Into single file             |
             output directory        release architecture      impreoves startup performance                        Into a single file

Go to https://github.com/JBrosDevelopment/Norma/blob/main/README.md to get started!


for value in ["Have", "Fun"] {
    print $value$
}