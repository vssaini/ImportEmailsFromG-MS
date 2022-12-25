'use strict';

var app = {

    init: function (gAuthStatus) {
        console.log(`Initiating app`);
        if (gAuthStatus === 'True') {
            $('#btnAuthorizeGmail').addClass('d-none');
            $('#btnImportGmail').removeClass('d-none');
        } else {
            $('#btnAuthorizeGmail').removeClass('d-none');
            $('#btnImportGmail').addClass('d-none');
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

    bindRows: function(data) {

        $.each (data, function( index, item ) {

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