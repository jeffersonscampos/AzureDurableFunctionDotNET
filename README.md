"# AzureDurableFunctionDotNET" 

# OK Funcionando no Azure:

# Simulando Pedido a ser aprovado (Regra menor que R$ 500.000,00):
# https://funcaoaprovarpedidonova3.azurewebsites.net/api/Function1_HttpStart?idPedido=1758&valor=499123

# Simulando Pedido a ser reprovado (Regra maior que R$ 500.000,00):
# https://funcaoaprovarpedidonova3.azurewebsites.net/api/Function1_HttpStart?idPedido=1758&valor=500123

# Teste da pipeline:
# Fazer POST para:
https://projeto2-api.azurewebsites.net/Api/login
com payload JSON: {"nome": "xxx", "senha": "xxx"}

