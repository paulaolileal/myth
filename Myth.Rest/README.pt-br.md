# Myth.Rest

![NuGet Version](https://img.shields.io/nuget/v/Myth.rest?style=for-the-badge&logo=nuget) ![NuGet Version](https://img.shields.io/nuget/vpre/Myth.rest?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

É uma biblioteca .NET para consumir como cliente de API's REST. O principal objetivo é simplificar o consumo e podendo trabalhar o RESTFULL.

Para utilizar é muito simples. Basta fazer encadeamento de ações para montar sua requisição.

# ⭐ Funcionalidades

- Uso simples
- Ações encadeadas
- Trabalha com arquivos
- Muito personalizável
- Re-aproveitável para multiplas requisições
- Orientado a exceções

# 🕶️ Usando

Essa biblioteca é preparada para tratar requisições de conteúdo de texto ou arquivos. Cada uma será vista abaixo.

## 📄 Requisições de conteúdo

Uma requisição de contúdo é uma requisição que envia e ou recebe arquivos de texto. Trabalha com diversos formatos.

Para iniciar uma requisição de conteúdo utilize:

```csharp
Rest.Create()
```

Exemplo de uma requisição completa:

```csharp
var response = await Rest
	.Create( )													// Inicializa a requisição
  	.Configure( config => config								// Configurações padrões
		.WithBaseUrl( "https://localhost:5001/" )				// Define a url base
		.WithContentType( "application/json" )					// Define o tipo de conteúdo
		.WithBodySerialization( CaseStrategy.CamelCase )		// Define o tipo de serialização do corpo da requisição
		.WithBodyDeserialization( CaseStrategy.SnakeCase ) )	// Define o tipo de serialização do corpo da resposta
  	.DoGet( "get-success" )										// Define a ação a ser realizada `get`, `post`, `put`, `patch`, `delete`
  	.OnResult( config => config									// Define o que deve acontecer em caso de sucesso
    	.UseTypeForSuccess<IEnumerable<Post>>( ) )				// ... nesse caso: sempre que for sucesso, status code >= 200 && < 299, usar o tipo `IEnumerable<Post>`
  	.OnError( error => error									// Define o que deve acontecer em caso de erro
		.ThrowForNonSuccess( ) )								// ... nesse caso: sempre que for diferente de sucesso, lançar uma exceção
  	.BuildAsync( );												// Executa a requisição
```

### ⚙️ Pré-configurando a requisição

O `.Configure( ... )` é a porta de entrada para a configuração da requisição. Muitas coisas podem ser definidas para facilitar, segue abaixo as funcionalidades:

- `.WithBaseUrl( param: string)`: Recebe a base da URL a ser requisitada. Exemplo: `https://test.com/testing`. A base seria `htps://test.com/`
- `.WithContentType( param: string )`: Recebe o tipo do conteúdo a ser recebido. Exemplo: `application/json`
- `.WithBodySerialization( param: CaseStrategy )`: Define como o json do corpo da sua requisição deve ser construído. Exemplo: `CaseStrategy.CamelCase`: `{ "myProp": "test" }` ou `CaseStrategy.SnakeCase`: `{ "my_prop": "test" }`
- `.WithBodyDeserialization( param: CaseStrategy )`: Define como o json do corpo da resposta deve ser lido.
- `.WithTimeout( param: TimeSpan )`: Determina qual o tempo máximo para se aguardar uma requisição.
- `.WithAuthorization( param: string, param: string)`: Adiciona um header de autorização personalizado a partir de um esquema e um token.
- `.WithBearerAuthorization( param: string)`: Adiciona um header de autorização do tipo Bearer com o token informado.
- `.WithBasicAuthorization( param: string, param: string)`: Adiciona um header de autorização do tipo Basic a partir do usuário e senha informados.
- `.AddHeader( param: string, param string, param: bool )`: Adiciona outros headers necessários para requisição a partir de chave e valor.
- `.WithClient( param: HttpClient)`: Adiciona um cliente http previamente configurado.

### 🔮 Realizando ações

Podem ser realizados todos os tipos de ações esperadas pelo REST.

- `.DoGet( param: string)`: Realiza um GET na rota informada.
- `.DoDelete( param: string)`: Realiza um DELETE na rota informada.
- `.DoPost<TBody>( param: string, param: TBody)`: Realiza um POST na rota informada, enviando o _body_ serializado.
- `.DoPut<TBody>( param: string, param: TBody)`: Realiza um PUT na rota informada, enviando o _body_ serializado.
- `.DoPatch<TBody>( param: string, param: TBody)`: Realiza um PATCH na rota informada, enviando o _body_ serializado.

### ✔️ Tratando resultados

É possivel tratar tipagem para diferentes status codes.

>  É possível utilizar uma condição para avaliar se esse tipo deve ser usado.

- `.UseTypeForSuccess<TResult>( param: Func<dynamic, bool>? )`: Usa um tipo definido para todos os status code de sucesso. 
- `.UseTypeFor( param: HttpStatusCode, param: Func<dynamic, bool>? )`: Usa um tipo definido para um status code especifico.
- `.UseEmptyFor( param: HttpStatusCode, param: Func<dynamic, bool>? )`: Define um corpo vazio para um status code especifico. Exemplo: `204 NoContent`
- `.UseTypeFor<TResult( param: IEnumerable<HttpStatusCode>, param: Func<dynamic, bool>? )`: Usa um tipo definido para uma lista de status codes.
- `.UseTypeForAll<TResult>( param: Func<dynamic, bool>? )`: Define para todos os status codes o mesmo tipo.

### ❌ Tratando erros

É possível definir para qual status code deve lançar exceções. A exceção a ser lançada em todos os casos em que o erro é o esperado é a `NonSuccessException`.

>  Tambem é possível utilizar uma condição para avaliar se esse tipo deve ser usado.

- `.ThrowForNonSuccess( param: Func<dynamic, bool>? )`: Lança a exceção para todos os status que não forem de sucesso.
- `.ThrowFor( param: HttpStatusCode, param: Func<dynamic, bool>? )`: Lança exceção para o status code definido 
- `.ThrowForAll( param: Func<dynamic, bool>? )`: Lança exceção para todos os status codes.
- `.NotThrowForNonMappedResult()`: Não lança exceção se não existir um tipo para o status code recebido
- `.NotThrowFor( param: HttpStatusCode, param: Func<dynamic, bool>? )`: Não lança exceção para um status code definido. 

## 📁 Requisições de arquivos

Essa biblioteca também permite e facilita trabalhar com arquivos, fazendo _download_ e _upload_.

Para iniciar uma requisição de arquivos utilize:

```csharp
Rest.File()
```

### ⬇️ Realizando downloads

Para realizar um download basta utilizar o `.DoDownload( param: string )`. Todas as configurações e tratamentos de erros mantém da mesmo forma que requisições de conteúdo. Segue um exemplo:

```csharp
var response = await Rest								
	.File( )													// Define como requisição de arquivo
	.Configure( conf => conf									// Pré-configura a requisição
		.WithBaseUrl( "https://localhost:5001" ) );				// Define qual a URL base a ser utilizada
	.DoDownload( "download-success" )							// Define a ação de download com a URL a ser utilizada
	.OnError( error => error									// Define o que fazer em caso de erros
		.ThrowForNonSuccess( ) )								// Sempre que der erro, lance exceções
	.BuildAsync( );												// Execute a requisição

await response.SaveToFileAsync( directory, fileName, true );	// Salva o arquivo baixado em um diretório da máquina

response.ToStream();											// Retorna um stream a ser utilizado posteriormente
```

### ⬆️ Realizando uploads

Os _uploads_ seguem o mesmo padrão do _download_. Muda somente a ação para `.DoUpload( param: string, param: File )`.

```csharp
var response = await Rest
	.File( )													// Define como requisição de arquivo
	.Configure( conf => conf									// Pré-configura a requisição
		.WithBaseUrl( "https://localhost:5001" ) )				// Define qual a URL base a ser utilizada
	.DoUpload( "upload-success", file )							// Define a ação de download com a URL a ser utilizada
	.OnError( error => error									// Define o que fazer em caso de erros
		.ThrowForNonSuccess( ) )								// Sempre que der erro, lance exceções
	.BuildAsync( );												// Execute a requisição
```

Uploads podem utilizar ações diferentes e para isso basta seguir o exemplo:

```csharp
...
	.DoUpload("upload-success", file, settings => settings.UsePutAsMethod() )
...
```

Podem ser utlizados:
- `.UsePostAsMethod()`: Default
- `.UsePutAsMethod()`
- `.UsePatchAsMethod()`

# ⚡ Outros casos de uso

## API's que sempre retornam 200 OK

Para aqueles pessimos casos, em que uma API sempre retorna 200 OK e o corpo da resposta vai definir se realmente foi sucesso ou não. Podemos fazer da seguinte forma:

Considerando uma resposta do seguinte padrão:
```json
{
	"code": 01,
	"success": true,
	"message": "This is a message"
}
```

A requisição deve avaliar a propriedade `success` para saber se realmente foi um erro. E para isso fazemos o seguinte:

```csharp
var response = await Rest
	.Create( )													
  	.Configure( config => config								
		.WithBaseUrl( "https://localhost:5001/" )				
  	.DoGet( "route" )										
  	.OnResult( config => config								
    	.UseTypeFor<ResponseType>( 
			HttpStatusCode.OK, 
			body => body.success == true ) )				
  	.OnError( error => error								
		.ThrowFor( HttpStatusCode.OK, body => body.success == false )
		.ThrowForNonSuccess( ) )							
  	.BuildAsync( );			
```

Assim se `success` for `true`, a resposta será gerada. Se não, uma exceção `NonSuccessException` será lançada.

## Construção de um repositório

Para reaproveitar em multiplas requisições as mesmas configurações, pode ser feito da seguinte forma:

```csharp

public class Test{
	private readonly RestBuilder _client;

	public Test(){
		_client = Rest
			.Create()
			.Configure( conf => conf 
				.UseBaseUrl("https://localhost:5001")
				.WithContentType( "application/json" )					
				.WithBodySerialization( CaseStrategy.CamelCase )		
				.WithBodyDeserialization( CaseStrategy.SnakeCase ) );	
	}

	public async Task<ResponseType> GetTestAsync(CancellationToken cancellationToken){
		var response = await _client
			.DoGet("route")
			.OnResult( config => config									
				.UseTypeForSuccess<ResponseType>())			
			.OnError( error => error									
				.ThrowForNonSuccess( ) )								
			.BuildAsync( cancellationToken );	
				
		return response.GetAs<ResponseType>();
	}

	public async Task PostTestAsync(RequestType request, CancellationToken cancellationToken){
		await _client
			.DoPost("route", request)
			.OnResult( config => config									
				.UseEmptyFor(HttpStatusCode.NoContent))				
			.OnError( error => error									
				.ThrowForNonSuccess( ) )								
			.BuildAsync( cancellationToken );
	}
}
```