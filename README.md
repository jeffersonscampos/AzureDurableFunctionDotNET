"# AzureDurableFunctionDotNET" 

# Testes no Azure:

# Simulando Pedido a ser aprovado (Regra menor que R$ 500.000,00):
# https://funcaoaprovarpedidonova3.azurewebsites.net/api/Function1_HttpStart?idPedido=1758&valor=499123

# Simulando Pedido a ser reprovado (Regra maior que R$ 500.000,00):
# https://funcaoaprovarpedidonova3.azurewebsites.net/api/Function1_HttpStart?idPedido=1758&valor=500123

# Testes Localmente: 

# Simulando Pedido a ser aprovado (Regra menor que R$ 500.000,00):
# http://localhost:7285/api/HttpStart?idPedido=1758&valor=499123

# Simulando Pedido a ser reprovado (Regra maior que R$ 500.000,00):
# http://localhost:7285/api/HttpStart?idPedido=1758&valor=500123


![2024-03-08 - Azure Function no Portal - print 001](https://github.com/jeffersonscampos/AzureDurableFunctionDotNET/assets/10118943/338a8ba2-2ec1-4242-874c-a68ead197147)

![2024-03-08 - Azure Function no Portal - print 002](https://github.com/jeffersonscampos/AzureDurableFunctionDotNET/assets/10118943/9239ec6a-7882-491f-baa5-af63318f5e41)

