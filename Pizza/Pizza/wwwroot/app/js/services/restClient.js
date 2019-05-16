;(function(services, $) {

    const ajaxUtil = window.util.ajax;
    const valueStorage = window.services.valueStorage;
    const tokenKey = "token";

    function login(userName, pwd) {
        return ajaxUtil.ajax("POST", "/api/tokens", { username: userName, password: pwd}).done(function (token) {
            valueStorage.setItem(tokenKey, token.token);
        });
    }

    function logout() {
        valueStorage.setItem(tokenKey, undefined);
        return $.Deferred().resolve().promise();
    }

    function createPizza(pizzeName) {
        return ajaxUtil.ajax("POST", "/api/orders", {name: pizzeName}, {authorization: "Bearer " + valueStorage.getItem(tokenKey)});
    }

    function isLoggedIn() {
        return !!valueStorage.getItem(tokenKey);
    }

    function getOrders() {
        return ajaxUtil.ajax("GET", "/api/orders", undefined, {authorization: "Bearer " + valueStorage.getItem(tokenKey)});
    }

    function getOrder(id) {
        return ajaxUtil.ajax("GET", `/api/orders/${id}`, undefined, {authorization: "Bearer " + valueStorage.getItem(tokenKey)});
    }

    function deleteOrder(id) {
        return ajaxUtil.ajax("DELETE", `/api/orders/${id}`, undefined, {authorization: "Bearer " + valueStorage.getItem(tokenKey)});
    }

    services.restClient = {
        login: login,
        logout: logout,
        createPizza: createPizza,
        isLogin: isLoggedIn,
        getOrders,
        getOrder,
        deleteOrder
    };
}(window.services = window.services || { }, jQuery));