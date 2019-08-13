--region Base64.lua
--Date 2016/05/06
--Base64算法加解密
--Author Norman
--endregion

function ToBase64(source_str)
    local b64chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'
    local s64 = ''
    local str = source_str

    while #str > 0 do
        local bytes_num = 0
        local buf = 0

        for byte_cnt=1,3 do
            buf = (buf * 256)
            if #str > 0 then
                buf = buf + string.byte(str, 1, 1)
                str = string.sub(str, 2)
                bytes_num = bytes_num + 1
            end
        end

        for group_cnt = 1,(bytes_num + 1) do
            b64char = math.fmod( math.floor( buf / 262144 ), 64 ) + 1
            s64 = s64 .. string.sub(b64chars, b64char, b64char)
            buf = buf * 64
        end

        for fill_cnt=1,(3-bytes_num) do
            s64 = s64 .. '='
        end
    end

    return s64
end

function FromBase64(str64)
    local b64chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'
    local temp={}
    for i = 1, 64 do
        temp[string.sub(b64chars, i, i)] = i
    end
    temp['='] = 0
    local str = ""
    for i = 1, #str64, 4 do
        if i > #str64 then
            break
        end
        local data = 0
        local str_count=0
        for j = 0, 3 do
            local str1=string.sub(str64, i+j, i+j)
            if not temp[str1] then
                return
            end
            if temp[str1] < 1 then
                data = data * 64
            else
                data = data * 64 + temp[str1] - 1
                str_count = str_count + 1
            end
        end
        for j=16, 0, -8 do
            if str_count > 0 then
                str = str .. string.char( math.floor( data / math.pow(2,j) ) )
                data = math.modf( data, math.pow(2,j) )
                str_count = str_count - 1
            end
        end
    end
    return str
end