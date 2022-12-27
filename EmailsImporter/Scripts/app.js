'use strict';

var app = {

    init: function (gAuthStatus, msAuthStatus) {
        console.log(`Initiating app`);
        app.setGmailBtnState(gAuthStatus);
        app.setMsMailBtnState(msAuthStatus);
    },

    setGmailBtnState: (gAuthStatus) => {
        if (gAuthStatus === 'True') {
            $('#btnAuthorizeGmail').addClass('d-none');
            $('#btnImportGmail').removeClass('d-none');
        } else {
            $('#btnAuthorizeGmail').removeClass('d-none');
            $('#btnImportGmail').addClass('d-none');
        }
    },

    setMsMailBtnState: (msAuthStatus) => {
        if (msAuthStatus === 'True') {
            $('#btnAuthorizeMsMail').addClass('d-none');
            $('#btnImportMsMail').removeClass('d-none');
        } else {
            $('#btnAuthorizeMsMail').removeClass('d-none');
            $('#btnImportMsMail').addClass('d-none');
        }
    },

    fetchGoogleEmails: function () {

        const btnId = '#btnImportGmail';
        const btnDefaultTxt = 'Import from Gmail';
        const defaultErrorMsg = 'Error occurred while fetching emails';

        showLoader('#container');
        showMessage('Retrieving emails from Gmail', 'info');
        changeButtonState(btnId, 'Importing...', true);

        const successCallback = (response) => {
            changeButtonState(btnId, btnDefaultTxt, false);

            switch (response.Type) {
            case 'Ok':
                if (response.RedirectUri) {
                    console.log(response.RedirectUri);
                    window.location.href = response.RedirectUri;
                }
                else {
                    showMessage('Emails retrieved successfully!', 'success');
                    $('#container').html(response.Data);
                }
                break;

            case 'Error':
                showMessage(response.ErrorMessage, 'error');
                $('#container').html(response.ErrorMessage);
                break;
            }
        }

        const errorCallback = (jqXhr, status, error) => {
            console.error(jqXhr);
            changeButtonState(btnId, btnDefaultTxt, false);

            const errorMsg = formatErrorMessage(jqXhr, error);
            const msg = errorMsg ? errorMsg : defaultErrorMsg;
            showMessage(msg, 'error');
        };

        ajaxCall('GET', '/Google/GetEmails', successCallback, errorCallback);
    },

    fetchMicrosoftEmails: function () {

        const btnId = '#btnImportMsMail';
        const btnDefaultTxt = 'Import from Microsoft';
        const defaultErrorMsg = 'Error occurred while fetching emails';

        showLoader('#container');
        showMessage('Retrieving emails from Microsoft Mail', 'info');
        changeButtonState(btnId, 'Importing...', true);

        const successCallback = (response) => {
            changeButtonState(btnId, btnDefaultTxt, false);

            switch (response.Type) {
                case 'Ok':
                    if (response.RedirectUri) {
                        console.log(response.RedirectUri);
                        window.location.href = response.RedirectUri;
                    }
                    else {
                        showMessage('Emails retrieved successfully!', 'success');
                        $('#container').html(response.Data);
                    }
                    break;

                case 'Error':
                    showMessage(response.ErrorMessage, 'error');
                    $('#container').html(response.ErrorMessage);
                    break;
            }
        }

        const errorCallback = (jqXhr, status, error) => {
            console.error(jqXhr);
            changeButtonState(btnId, btnDefaultTxt, false);

            const errorMsg = formatErrorMessage(jqXhr, error);
            const msg = errorMsg ? errorMsg : defaultErrorMsg;
            showMessage(msg, 'error');
        };

        ajaxCall('GET', '/Microsoft/GetEmails', successCallback, errorCallback);
    },

    bindRows: function (data) {

        $.each(data, function (index, item) {

            const row = `<tr>
                         <td>${item.Date}</td>
                         <td>${item.From}</td>
                         <td>${item.Subject}</td>
                         <td>${item.Attachments.length}</td>
                         </tr>`;

            $('#tblGmailEmails tr:last').after(row);
        });
    }
}