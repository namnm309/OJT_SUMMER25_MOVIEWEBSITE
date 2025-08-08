

// Xử lý hiển thị ngày tháng
function formatDate(dateString) {
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
}

// Xử lý validate form
$(document).ready(function () {
    $('form').validate({
        rules: {
            Title: "required",
            DiscountPercent: {
                required: true,
                min: 0,
                max: 100
            },
            StartDate: "required",
            EndDate: {
                required: true,
                greaterThan: "#StartDate"
            }
        },
        messages: {
            Title: "Vui lòng nhập tên khuyến mãi",
            DiscountPercent: {
                required: "Vui lòng nhập phần trăm giảm giá",
                min: "Phần trăm giảm giá phải từ 0% trở lên",
                max: "Phần trăm giảm giá không được vượt quá 100%"
            },
            StartDate: "Vui lòng chọn ngày bắt đầu",
            EndDate: {
                required: "Vui lòng chọn ngày kết thúc",
                greaterThan: "Ngày kết thúc phải sau ngày bắt đầu"
            }
        }
    });

    $.validator.addMethod("greaterThan", function (value, element, params) {
        if (!/Invalid|NaN/.test(new Date(value))) {
            return new Date(value) > new Date($(params).val());
        }
        return isNaN(value) && isNaN($(params).val())
            || (Number(value) > Number($(params).val()));
    }, 'Ngày kết thúc phải sau ngày bắt đầu');
});