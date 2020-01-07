# SalesImporter
Serviço de importação de arquivos de vendas em um formato pré-determinado.


#INSTALANDO O SERVIÇO NO WINDOWS 

Para instalar o serviço no windows deve ser seguido os seguintes passos:

1 - Gerar o build (Pode ser pelo Visual Studio ou por linha de comando)

2 - Copiar para o diretorio onde o mesmo vai ser instalado. 
(Exemplo: "C:\SalesImporter\")

3 - Alterar parametros do arquivo de configuração "SalesImporter.exe.config" 
(logo abaixo está a lista com cada parâmetro)

4 - Executar o comando "InstallUtil.exe para instalar o serviço no windows. 
(o programa InstallUtil.exe fica na pasta do framework .NET "C:\Windows\Microsoft.NET\Framework\v4.0.30319\" podendo mudar conforme versão instalada)
(Exemplo: C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe C:\SalesImporter\SalesImporter.exe)

5 - Após executado o comando vai ser solicitado as credencias de usuário para iniciar o serviço

6 - Iniciar o serviço no windows. 
(abra o gerenciador de serviços com o comando "services.msc")

Após estes passos o serviço ficará rodando e lendo constantemente o diretório de entrada informado no arquivo de configuração.


# PARÂMETROS CONFIGURÁVEIS

- baseDir: Diretório base da aplicação onde serão criados os diretórios de entrada e saida do serviço
( Caso o mesmo esteja vazio o sistema utiliza a variável de ambiente HOMEDRIVE + HOMEPATH )

- logDir: Diretório onde ficam os arquivos de Log. Criado dentro do diretório base

- inputDir: Onde os arquivos ".dat" devem ser jogados para serem processados
- outputDir: Onde os relatórios pós processamento são gerados
- processedFilesDir: Onde os arquivos ".dat" processados são armazenados
- ignoredFilesDir: Onde os arquivos que não são do tipo ".dat" são jogados

- extentionFile: Extenção do arquivo processado (padrão ".dat")
- processedExtentionFile: Extenção do relatório gerado (padrão ".done.dat")

- separator: Caracter que separa as colunas dentro d arquivo (padrão "ç")

# separadores da linha de itens de vendas (exemplo: [1-10-100,2-30-2.50,3-40-3.10])
- itensInitialMark: Caracter que marca o inicio dos itens do pedido padrão "["
- itensFinalMark: Caracter que marca o fim dos itens do pedido padrão '['
- itensSeparator: Caracter que separa as colunas dos itens do pedido padrão ","
- itemSeparator: Caracter que separa as colunas de um item de pedido padrão "-"

# Relatório gerado

No final do processamento de cada arquivo é gerado um novo aquivo no formato JSON contendo os seguintes itens:

* Quantidade de clientes no arquivo de entrada "CustomerCount"
* Quantidade de vendedor no arquivo de entrada "SellerCount"
* ID(s) da(s) venda(s) mais cara(s) "ExpensiveSaleId"
* O(s) pior(es) vendedor(es) "WorstSaller"

Exemplo:
{"CustomerCount":2,"SellerCount":4,"ExpensiveSaleId":[25,28],"WorstSaller":["Pedro"]}


