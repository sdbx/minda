import React, { useState } from 'react';
import styled from 'styled-components';
import axios from 'axios';
import MainLayout from '../layouts/MainLayout';

const WrapperStyle = styled.div`
    text-align: center;
    font-size: 20px;
`;

const Stat: React.FC = () => {
    const [id, setId] = useState<string>('');
    const [data, setData] = useState<string>('유저ID를 이용하여 전적을 가져옵니다.');

    const onEnterKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            getStat();
        }
    };

    const onStatInputChange = (evt: React.ChangeEvent<HTMLInputElement>) => {
        setId(evt.target.value);
    };

    const getStat = () => {
        if (id === '') {
            alert('빈 칸이 있습니다.');
            return;
        }

        axios.get(`http://api.minda.games/histories/${id}`)
            .then((res) => {
                setData(res.data);
            })
            .catch((err) => {
                alert(err);
            });
    };

    return (
        <MainLayout>
            <WrapperStyle>
                당신의 스탯
                <br/>
                <input value={id} placeholder={'유저ID'} onChange={onStatInputChange}
                       onKeyPress={onEnterKeyPress}/>
                <button onClick={getStat}>조회</button>

                <br/>
                <br/>

                <p>{data}</p>
            </WrapperStyle>
        </MainLayout>
    );
};

export default Stat;
