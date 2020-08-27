
# Sociedade Serial

## 01 - Propósito
Software utilizado para execução de ensaios de interface de comunicação em instrumentos de medição.

## 02 - Instalação
Executar o "R:\Compartilhado\Sociedade Serial\setup.exe". Antes da inicialização do aplicativo é verificado automaticamente se existe novas atualizações.

## 03 - Tecnologias empregadas
Aplicação foi desenvolvida em C# com projeto de renderização utilizando Windows Presentation Foundation (WPF). Para a execução da comunicação serial foi utilizada a API nativa "System.IO.Ports" em conjunto com o "Newtonsoft.Json" para interpretação dos scripts que devem estar no formato .json. Além da thread responsável pelo processo de renderização, a aplicação possui uma thread responsável pelo recebimento dos dados pela porta serial e outra para o envio. 

## Alterações na versão 2.2
 - Modificado o elemento no qual são apresentados os resultados parciais para que possa-se copiar os dados;
 - Modificada a forma como inserir um script de tratamento de comando, para seleção de um arquivo;
 - Correção para tornar possível o uso da letra "V" no nome de Tags dinâmicas;






 

