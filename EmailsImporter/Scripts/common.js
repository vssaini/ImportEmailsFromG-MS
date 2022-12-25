'use strict';

/**
* Make AJAX request.
*/
var ajaxCall = function (requestType, url, successCallback, errorCallback, data = null, contentType = null, isAsync = false) {

    try {
        $.ajax({
            url: url,
            type: requestType,
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: contentType ? contentType : 'application/json',
            success: successCallback,
            error: errorCallback,
            async: isAsync ? isAsync : true
        });
    } catch (err) {
        showMessage(`${err.name} : ${err.message}`, null, 'error');
    }
};

/**
* Format error message based on status code and error thrown.
*/
var formatErrorMessage = function (jqXhr, error) {
    if (jqXhr.status === 0) {
        return ('Not connected.\nPlease verify your network connection.');
    } else if (jqXhr.status === 404) {
        return ('The requested page not found.');
    } else if (jqXhr.status === 401) {
        return ('Sorry!! Your session has expired. Please login to continue access.');
    } else if (jqXhr.status === 403) {
        return ('Access Denied – You don’t have permission to access.');
    } else if (jqXhr.status === 500) {
        return ('Internal Server Error.');
    } else if (error === 'parsererror') {
        return ('Requested JSON parse failed.');
    } else if (error === 'timeout') {
        return ('Time out error.');
    } else if (error === 'abort') {
        return ('Ajax request aborted.');
    } else {
        if (error) {
            return error;
        } else {
            return null;
        }
    }
};

/**
* Show toaster message as per type.
* @param {any} message
* @param {'Allow only success, error, warning and info'} type
* @param {any} headline
*/
var showMessage = function (message, type, headline = null) {
    switch (type) {
        case 'success':
            toastr.success(
                message,
                headline,
                { timeOut: 5000, extendedTimeOut: 1000, closeButton: true, closeDuration: 3000 }
            );
            break;

        case 'error':
            toastr.error(
                message,
                headline,
                { timeOut: 5000, extendedTimeOut: 1000, closeButton: true, closeDuration: 3000 }
            );
            break;

        case 'warning':
            toastr.warning(
                message,
                headline,
                { timeOut: 5000, extendedTimeOut: 1000, closeButton: true, closeDuration: 3000 }
            );
            break;

        case 'info':
            toastr.info(
                message,
                headline,
                { timeOut: 5000, extendedTimeOut: 1000, closeButton: true, closeDuration: 3000 }
            );
            break;
    }
};

/**
* Show loading icon in specified element Id.
* @param {any} targetEleId - The target element Id or class that will be used for displaying loader.
*/
var showLoader = function(targetEleId) {
    $(targetEleId).html(`
            <div class="loader" style="text-align: center;">
                <img src="/Content/img/loader.jpg" style="height: 50px;margin-bottom: 20px;" />
            </div>
        `);
};

/**
* Change button text and disable state.
*/
var changeButtonState = function (eleId, buttonTxt, disable) {

    $(eleId).attr('disabled', disable);
    $(eleId).text(buttonTxt);
};