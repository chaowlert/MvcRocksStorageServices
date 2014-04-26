function TodoCtrl($scope, $http) {
    $http.get('/api/todo/chaow').success(function (info) {
        $scope.info = info;
        $http.get($scope.info.uri + $scope.info.sas).success(function (todos) {
            $scope.todos = todos.value;
        });
    });

    $scope.todos = [];

    $scope.getTotalTodos = function () {
        return $scope.todos.length;
    };

    $scope.addTodo = function () {
        var data = {
            PartitionKey: $scope.info.id,
            RowKey: new Date().getTime().toString(),
            text: $scope.formTodoText,
            done: false
        };
        $scope.todos.push(data);
        $http.post($scope.info.uri + $scope.info.sas, data);
        $scope.formTodoText = '';
    };

    $scope.check = function(todo) {
        var data = { done: todo.done };
        var uri = $scope.info.uri + "(PartitionKey='" + $scope.info.id + "',RowKey='" + todo.RowKey + "')" + $scope.info.sas;
        var config = {
            headers: {
                'X-HTTP-Method': 'MERGE',
                'If-Match': '*'
            }
        };
        $http.post(uri, data, config);
    };

    $scope.clearCompleted = function () {
        for (var i = $scope.todos.length - 1; i >= 0; i--) {
            var todo = $scope.todos[i];
            if (!todo.done)
                continue;
            $scope.todos.splice(i, 1);
            var uri = $scope.info.uri + "(PartitionKey='" + $scope.info.id + "',RowKey='" + todo.RowKey + "')" + $scope.info.sas;
            var config = {
                headers: { 'If-Match': '*' }
            }
            $http.delete(uri, config);
        }
    };
}