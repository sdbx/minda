FROM rustlang/rust:nightly

WORKDIR /minda

COPY Cargo.lock .
COPY Cargo.toml .
COPY dummy.rs .

RUN sed -i 's@src/main.rs@dummy.rs@' Cargo.toml
RUN cargo build --release
RUN sed -i 's@dummy.rs@src/main.rs@' Cargo.toml
COPY . .
RUN cargo build --release

ENTRYPOINT ["/minda/target/release/game"]