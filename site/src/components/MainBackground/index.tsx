import React from 'react';
import styled from 'styled-components';
import MindaMotto from '../../atoms/MindaMotto';

const WrapperStyle = styled.div`
    min-width: 100%;
    height: 470px;
    background-color: gray;
`;

const ButtonStyle = styled.button`
    margin-top: 12px;
    
    width: 140px;
    height: 36px;
    border-radius: 50px;
    font-size: 18px;
    color: white;
    background-color: #0984e3;
`;

const MainBackground: React.FC = () => {
    return (
        <WrapperStyle className="flex items-center justify-center">
            <div>
                <MindaMotto />
                <ButtonStyle>asd</ButtonStyle>
            </div>
        </WrapperStyle>
    );
};

export default MainBackground;
